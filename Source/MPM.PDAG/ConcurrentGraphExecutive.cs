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
using System.Linq;
using System.Threading;

namespace MPM.PDAG
{
    /// <summary>
    /// Executes a graph concurrently.
    /// </summary>
    public class ConcurrentGraphExecutive : IGraphExecutive
    {
        private readonly object _executionLock = new Object();
        private readonly DirectedAcyclicGraph _graph;
        private readonly IConcurrencyThottleStrategy _throttle;
        private readonly object _scheduleLock = new object();
        private List<Vertex> _verticesComplete;
        private int _verticesProcessed, _allVerticesCount;

        public IDictionary<Vertex, Exception> VerticesFailed { get; private set; }
        public event EventHandler OnGraphExecutionFinished; 

        /// <summary>
        /// Executes a graph concurrently.
        /// </summary>
        /// <param name="graph">The graph to execute</param>
        /// <param name="throttle">optional concurrency throttle</param>
        /// <param name="concurrencyThrottleStrategy">optional ConcurrencyThrottleStrategy</param>
        public ConcurrentGraphExecutive(DirectedAcyclicGraph graph, ConcurrencyThrottle throttle = null, ConcurrencyThrottleStrategy concurrencyThrottleStrategy = ConcurrencyThrottleStrategy.PostThreadQueue)
        {
            _graph = graph;
            _throttle = new ConcurrencyThrottleStrategyFactory(throttle ?? new NullConcurrencyThrottle()).Build(concurrencyThrottleStrategy);
        }

        /// <summary>
        /// Executes the graph and returns once execution is complete.
        /// </summary>
        public void ExecuteAndWait()
        {
            var resetEvent = new ManualResetEventSlim();

            OnGraphExecutionFinished += (sender, ergs) => resetEvent.Set();

            Execute();

            resetEvent.Wait();
        }

        /// <summary>
        /// Executes the graph and returns immediately, unless ConcurrencyThrottleStrategy.PreThreadQueue strategy is specified in which case it will return once all vertices are scheduled
        /// </summary>
        public void Execute()
        {
            lock (_executionLock)
            {
                _verticesComplete = new List<Vertex>();
                VerticesFailed = new Dictionary<Vertex, Exception>();
                _verticesProcessed = 0;
                _allVerticesCount = _graph.AllVertices.Count();
                foreach (var vertex in _graph.RootVertices)
                    Execute(vertex);
            }
        }

        private void Execute(Vertex vertex)
        {
            _throttle.EnterPreThreadQueue();
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    _throttle.EnterPostThreadQueue();
                    vertex.Execute();
                   
                    ProcessPostVertexExecutionSuccess(vertex);
                }
                catch (Exception ex)
                {
                    ProcessPostVertexExecutionFailure(vertex, ex);
                }
                finally
                {
                    _throttle.Exit();
                }
            });
        }

        private void ProcessPostVertexExecutionSuccess(Vertex vertex)
        {
            IEnumerable<Vertex> next;
            lock (_scheduleLock)
            {
                _verticesComplete.Add(vertex);
                _verticesProcessed++;

                next =
                    vertex.Dependents.Where(
                        d => d.Dependencies.All(n => _verticesComplete.Contains(n) && !_verticesComplete.Contains(d)));
                    
                if (_verticesProcessed == _allVerticesCount && OnGraphExecutionFinished != null)
                    OnGraphExecutionFinished(this, EventArgs.Empty);
            }

            foreach (var v in next)
                Execute(v);
        }

        private void ProcessPostVertexExecutionFailure(Vertex vertex, Exception exception)
        {
            lock (_scheduleLock)
            {
                _verticesComplete.Add(vertex);
                VerticesFailed.Add(vertex, exception);
                _verticesProcessed++;

                foreach (var dependent in vertex.Dependents.FlattenAndGetDistinct())
                {
                    _verticesComplete.Add(dependent);
                    VerticesFailed.Add(dependent, new Exception("Dependency Failed"));
                    _verticesProcessed++;
                }
            }
        }
    }
}
