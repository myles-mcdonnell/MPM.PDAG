using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace MPM.PDAG
{
    public class ConcurrentGraphExecutive : IGraphExecutive
    {
        private readonly object _executionLock = new Object();
        private readonly DirectedAcyclicGraph _graph;
        private readonly IConcurrencyThottleStrategy _throttle;

        private List<Vertex> _verticesComplete;
        
        private int _verticesProcessed, _allVerticesCount;

        public IDictionary<Vertex, Exception> VerticesFailed { get; private set; }
        public event EventHandler OnGraphExecutionFinished; 

        public ConcurrentGraphExecutive(DirectedAcyclicGraph graph, ConcurrencyThrottle throttle = null, ConcurrencyThrottleStrategy concurrencyThrottleStrategy = ConcurrencyThrottleStrategy.PostThreadQueue)
        {
            _graph = graph;
            _throttle = new ConcurrencyThrottleStrategyFactory(throttle ?? new NullConcurrencyThrottle()).Build(concurrencyThrottleStrategy);
        }

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
                    _verticesComplete.Add(vertex);
                    _verticesProcessed++;
                    ExecuteDependents(vertex);
                }
                catch (Exception ex)
                {
                    VerticesFailed.Add(vertex, ex);
                    _verticesProcessed++;

                    foreach (var dependent in vertex.Dependents.FlattenAndGetDistinct())
                    {
                        VerticesFailed.Add(dependent, new Exception("Dependency Failed"));
                        _verticesProcessed++;
                    }

                    throw;
                }
                finally
                {
                    _throttle.Exit();
                }
            });
        }

        public void ExecuteDependents(Vertex vertex)
        {
            foreach (var dependent in vertex.Dependents.Where(d => d.Dependencies.All(n => _verticesComplete.Contains(n))))
                Execute(dependent);

            if (_verticesProcessed==_allVerticesCount&&OnGraphExecutionFinished!=null)
                OnGraphExecutionFinished(this, EventArgs.Empty);
        }
    }
}
