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

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace MPM.PDAG
{
    public class GraphExecutive : IGraphExecutive
    {
        public event EventHandler<EventArgs<TimeSpan>> OnFinished;
        public event EventHandler<EventArgs<Tuple<TimeSpan, Exception>>> OnFailed;
        public event EventHandler OnStarted;
        public event EventHandler OnResumed;
        public event EventHandler OnResumePending;
        public event EventHandler OnPaused;
        public event EventHandler OnPausePending;
        public event EventHandler OnCancelled;
        public event EventHandler OnCancelPending;
        public event EventHandler<VertexProgressEventArgs> OnVertexProgress;

        private readonly IDirectedAcyclicGraph _graph;
        private readonly IConcurrencyThrottle _throttle;
        private readonly List<IVertex> _processedVertices = new List<IVertex>();
        private readonly object _lock = new object();

        private DateTime _start;

        public GraphExecutive(IDirectedAcyclicGraph graph, IConcurrencyThrottle throttle = null)
        {
            throttle = throttle ?? new ConcurrencyThrottle();

            _graph = graph;
            _throttle = throttle;
            State = GraphExecutiveState.Inactive;

            foreach (var vertex in graph.AllVertices)
            {
                vertex.OnCancelled += OnVertexCancelled;
                vertex.OnFailed += OnVertexFailed;
                vertex.OnCompleted += OnVertexComplete;
            }
        }

        void OnVertexCancelled(object sender, EventArgs<IVertex> eventArgs)
        {
            lock (_lock)
            {
                foreach (var dependent in eventArgs.Argument.Dependents)
                    AbandonVertex(dependent);

                ProcessVertexFinished(eventArgs.Argument);
            }
        }

        void OnVertexComplete(object sender, EventArgs<IVertex> eventArgs)
        {
            lock (_lock)
            {
                ProcessVertexFinished(eventArgs.Argument);
            }
        }

        void OnVertexFailed(object sender, EventArgs<Tuple<IVertex, Exception>> eventArgs)
        {
            lock (_lock)
            {
                foreach (var dependent in eventArgs.Argument.Item1.Dependents)
                    AbandonVertex(dependent);

                ProcessVertexFinished(eventArgs.Argument.Item1, eventArgs.Argument.Item2);
            }
        }

        void AbandonVertex(IVertex vertex)
        {
            foreach (var dependent in vertex.Dependents)
                AbandonVertex(dependent);

            _processedVertices.Add(vertex);
        }

        void ProcessVertexFinished(IVertex vertex, Exception ex = null)
        {
            _processedVertices.Add(vertex);
            
            if (_graph.AllVertices.All(v=>v.State==VertexState.Inactive) && State == GraphExecutiveState.CancelPending)
            {
                State = GraphExecutiveState.Inactive;
                LastExecutionResult = ExecutionResult.Cancel;

                if (OnCancelled != null)
                    OnCancelled(this, EventArgs.Empty);
            }
            else if (_processedVertices.Count == _graph.AllVertices.Count())
            {
                State = GraphExecutiveState.Inactive;

                if (ex==null)
                {
                    LastExecutionResult = ExecutionResult.Success;
                    if (OnFinished != null)
                        OnFinished(this, new EventArgs<TimeSpan>(DateTime.Now - _start));
                }
                else
                {
                    LastExecutionResult = ExecutionResult.Fail;
                    if (OnFailed != null)
                        OnFailed(this, new EventArgs<Tuple<TimeSpan, Exception>>(new Tuple<TimeSpan, Exception>(DateTime.Now - _start, ex)));
                }
            }
            else
            {
                foreach (var dependent in vertex.Dependents.Where(d=>!_processedVertices.Contains(d)))
                    dependent.Execute(this);
            }
        }

        public void Run()
        {
            lock(_lock)
            {
                if (State != GraphExecutiveState.Inactive) return;
                
                _processedVertices.Clear();
                _start = DateTime.Now;
                
                State = GraphExecutiveState.Running;

                if (OnStarted != null)
                    OnStarted(this, EventArgs.Empty);

                Parallel.ForEach(_graph.RootVertices, v => v.Execute(this));
            }
        }

        public void Pause()
        {
            lock (_lock)
            {
                if (State != GraphExecutiveState.Running) return;

                State=GraphExecutiveState.PausePending;

                if (OnPausePending != null)
                    OnPausePending(this, EventArgs.Empty);
            }
        }

        public void Resume()
        {
            lock (_lock)
            {
                if (State != GraphExecutiveState.Paused && State != GraphExecutiveState.PausePending) return;

                State = GraphExecutiveState.ResumePending;

                if (OnResumePending != null)
                    OnResumePending(this, EventArgs.Empty);

                Monitor.PulseAll(_lock);
            }
        }

        public void Cancel()
        {
            lock (_lock)
            {
                if (State == GraphExecutiveState.Inactive) return;

                if (OnCancelPending != null)
                    OnCancelPending(this, EventArgs.Empty);
                    
                State=GraphExecutiveState.CancelPending;

                Monitor.PulseAll(_lock);
            }
        }

        public bool CanVertexExecutionProceed()
        {
            lock (_lock)
            {
                if (State == GraphExecutiveState.CancelPending)
                    return false;

                _throttle.Enter();

                while (State == GraphExecutiveState.PausePending || State == GraphExecutiveState.Paused)
                {
                    if (State == GraphExecutiveState.PausePending && _graph.AllVertices.All(v => v.State == VertexState.Inactive))
                    {
                        State=GraphExecutiveState.Paused;
                       
                        if (OnPaused!=null)
                            OnPaused(this, EventArgs.Empty);
                    }

                    Monitor.Wait(_lock);

                    if (State==GraphExecutiveState.ResumePending)
                    {
                        State=GraphExecutiveState.Running;
                        if (OnResumed != null)
                            OnResumed(this, EventArgs.Empty);
                    }
                }

                return (State != GraphExecutiveState.CancelPending && State != GraphExecutiveState.CancelPending);
            }
        }

        public void SignalVertexExecutionEnd()
        {
            _throttle.Exit();
        }

        private GraphExecutiveState State { get; set; }

        public IDirectedAcyclicGraph Graph
        {
            get { return _graph; }
        }

        public ExecutionResult LastExecutionResult { get; private set; }
    }
}
