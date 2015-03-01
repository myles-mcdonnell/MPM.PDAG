#region HEADER
//   Copyright 2015 Myles McDonnell (mcdonnell.myles@gmail.com)

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//     http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
#endregion

#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MPM.PDAG.Impl;

#endregion

namespace MPM.PDAG
{
    public class Vertex : IVertex
    {
        public event EventHandler<EventArgs<IVertex>> OnStarted;
        public event EventHandler<EventArgs<IVertex>> OnCancelled;

        public event EventHandler<EventArgs<IVertex>> OnCompleted;
        public event EventHandler<EventArgs<Tuple<IVertex,Exception>>> OnFailed;
        public event EventHandler<VertexProgressEventArgs> OnProgress;
        public event EventHandler<EventArgs<IVertex>> OnRequiresReExecution;

        private delegate void RaiseEventDelegate();
        private delegate void RaiseOnProgressEventDelegate(VertexProgressEventArgs e);
        private delegate void RaiseOnFailedEventDelegate(Exception ex);

        private readonly RaiseEventDelegate _raiseOnStarted;
        private readonly RaiseEventDelegate _raiseOnCancelled;
        private readonly RaiseOnFailedEventDelegate _raiseOnFailed;
        private readonly RaiseEventDelegate _raiseOnCompleted;
        private readonly RaiseOnProgressEventDelegate _raiseOnProgress;

        private readonly Action<IVertexExecutionContext> _doWorkAction;
        private readonly ICollection<IVertex> _dependencies = new List<IVertex>();
        private readonly ICollection<IVertex> _dependents = new List<IVertex>();

        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ReaderWriterLockSlim _stateLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        private VertexState _state = VertexState.Inactive;
        private ExecutionResult _lastExecutionResult = ExecutionResult.None;

        public Vertex(Action<IVertexExecutionContext> doWorkAction, CancellationTokenSource cancellationTokenSource = null)
            :this(cancellationTokenSource)
        {
            _doWorkAction = doWorkAction;
        }

        public Vertex(CancellationTokenSource cancellationTokenSource = null)
        {
            Enabled = true;
            _cancellationTokenSource = cancellationTokenSource??new CancellationTokenSource();
            _raiseOnStarted = delegate {if (OnStarted != null) OnStarted(this, new EventArgs<IVertex>(this));};
            _raiseOnCancelled = delegate { if (OnCancelled != null) OnCancelled(this, new EventArgs<IVertex>(this)); };
            _raiseOnCompleted = delegate { if (OnCompleted != null) OnCompleted(this, new EventArgs<IVertex>(this)); };
            _raiseOnFailed = delegate (Exception ex) { if (OnFailed != null) OnFailed(this, new EventArgs<Tuple<IVertex, Exception>>(new Tuple<IVertex, Exception>(this, ex))); };
            _raiseOnProgress = delegate (VertexProgressEventArgs eventArgs){if (OnProgress != null) OnProgress(this, eventArgs);};
        }

        protected void ReportProgress(string message = "", int percentageComplete = -1)
        {
            RaiseOnProgress(new VertexProgressEventArgs(this, message, percentageComplete));
        }

        #region Raise Events

        private void RaiseOnStarted()
        {
            _raiseOnStarted.BeginInvoke(null, null);
        }
        
        private void RaiseOnCancelled()
        {
            _raiseOnCancelled.BeginInvoke(null, null);
        }
        
        private void RaiseOnCompleted()
        {
            _raiseOnCompleted.BeginInvoke( null, null);
        }
       
        private void RaiseOnFailed(Exception ex)
        {
            _raiseOnFailed.BeginInvoke(ex, null, null);
        }

        private void RaiseOnProgress(VertexProgressEventArgs eventArgs)
        {
            _raiseOnProgress.BeginInvoke(eventArgs, null, null);
        }

        #endregion

        #region INode Members

        public IDirectedAcyclicGraph GetGraph()
        {
            return new DirectedAcyclicGraph(this);
        }

        public bool IsDependency(IVertex vertex)
        {
            return Dependencies.Any(d => d == vertex) || (Dependencies.Any(dependency => dependency.IsDependency(vertex)));
        }

        public bool IsDependent(IVertex vertex)
        {
            return Dependents.Any(d => d == vertex) || (Dependents.Any(dependent => dependent.IsDependency(vertex)));
        }

        public string Name { get; set; }
        public object Tag { get; set; }

        public IEnumerable<IVertex> Dependencies
        {
            get { return _dependencies; }
        }

        public IVertex AddDependencies(params Vertex[] dependencies)
        {
            foreach (var dependency in dependencies)
                AddDependency(dependency);

            return this;
        }

        public IEnumerable<IVertex> Dependents
        {
            get { return _dependents; }
        }

        public void RemoveRedundantDependencies()
        {
            foreach (var dependency in Dependencies.FlattenAndGetDistinct(true).Where(dependency => _dependencies.Contains(dependency)))
                _dependencies.Remove(dependency);

            foreach (var dependent in Dependencies)
                dependent.RemoveRedundantDependencies();
        }

        public bool Enabled{ get; set; }

        public bool ContinueOnFailure { get; set; }

        #endregion

        private void AddDependency(Vertex vertex)
        {
            _stateLock.EnterWriteLock();
            try
            {
                if (vertex == this)
                    throw new CircularDependencyException(vertex);

                CheckForCircularDependency(vertex.Dependencies);

                if (_dependencies.Any(n => n == vertex))
                    return;

                _dependencies.Add(vertex);
                vertex._dependents.Add(this);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void CheckForCircularDependency(IEnumerable<IVertex> dependencies)
        {
            dependencies = dependencies.ToList();

            if (dependencies.Any(dependency => dependency == this))
                throw new CircularDependencyException(this);

            foreach (var dependency in dependencies)
                CheckForCircularDependency(dependency.Dependencies);
        }

        private void ProcessTaskCancelled(Task task)
        {
            _stateLock.EnterWriteLock();
            try
            {
                _state = VertexState.Inactive;
                _lastExecutionResult = ExecutionResult.Cancel;

                RaiseOnCancelled();
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void ProcessTaskFaulted(Task task)
        {
            _stateLock.EnterWriteLock();
            try
            {
                _state = VertexState.Inactive;
                _lastExecutionResult = ExecutionResult.Fail;

                RaiseOnFailed(task.Exception);
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        private void ProcessTaskCompletion(Task task)
        {
            _stateLock.EnterWriteLock();
            try
            {
                _state = VertexState.Inactive;
                _lastExecutionResult = ExecutionResult.Success;

                RaiseOnCompleted();
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }

        public VertexState State
        {
            get
            {
                _stateLock.EnterReadLock();
                try
                {
                    return _state;
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
        }

        public ExecutionResult LastExecutionResult
        {
            get
            {
                _stateLock.EnterReadLock();
                try
                {
                    return _lastExecutionResult;
                }
                finally
                {
                    _stateLock.ExitReadLock();
                }
            }
        }

        public Action<IVertexExecutionContext> DoWorkAction
        {
            get { return _doWorkAction; }
        }

        private void OnTaskStart(object state)
        {
            var graphExecutive = (IGraphExecutive) state;
            
            if (!graphExecutive.CanVertexExecutionProceed())
                return;

            _stateLock.EnterWriteLock();

            try
            {
                try
                {
                    _state = VertexState.Running;

                    RaiseOnStarted();
                }
                finally
                {
                    _stateLock.ExitWriteLock();
                }

                DoWork(new VertexExecutionContext(_cancellationTokenSource.Token, ReportProgress));
            }
            finally
            {
                graphExecutive.SignalVertexExecutionEnd();
            }
        }

        protected virtual void DoWork(IVertexExecutionContext vertexExecutionContext)
        {
            if (DoWorkAction != null) DoWorkAction(vertexExecutionContext);
        }

        public void Execute(IGraphExecutive graphExecutive)
        {
            var task = new Task(OnTaskStart, graphExecutive, _cancellationTokenSource.Token, TaskCreationOptions.None);

            task.ContinueWith(ProcessTaskCancelled, TaskContinuationOptions.OnlyOnCanceled); 
            task.ContinueWith(ProcessTaskFaulted, TaskContinuationOptions.OnlyOnFaulted);
            task.ContinueWith(ProcessTaskCompletion, TaskContinuationOptions.OnlyOnRanToCompletion);

            task.Start();
        }

        public void Cancel()
        {
            _stateLock.EnterWriteLock();
            try
            {
                if (State != VertexState.Running) return;
                
                _state = VertexState.CancellationPending;
                _cancellationTokenSource.Cancel();
                
            }
            finally
            {
                _stateLock.ExitWriteLock();
            }
        }
    }
}