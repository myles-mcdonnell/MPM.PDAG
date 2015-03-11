using System;
using System.Collections.Generic;
using System.Linq;

namespace MPM.PDAG
{
    public class SequentialGraphExecutive : IGraphExecutive
    {
        public IDictionary<Vertex, Exception> VerticesFailed { get; private set; }

        public event EventHandler OnGraphExecutionFinished;

        private readonly object _executionLock = new Object();
        private readonly DirectedAcyclicGraph _graph;
       
        private List<Vertex> _verticesComplete;

        public SequentialGraphExecutive(DirectedAcyclicGraph graph)
        {
            _graph = graph;
            VerticesFailed = new Dictionary<Vertex, Exception>();
        }

        public void Execute()
        {
            lock (_executionLock)
            {
                _verticesComplete = new List<Vertex>();
                foreach (var vertex in _graph.RootVertices)
                    Execute(vertex);

                if (OnGraphExecutionFinished != null)
                    OnGraphExecutionFinished(this, EventArgs.Empty);
            }
        }
        
        private void Execute(Vertex vertex)
        {
            try
            {
                vertex.Execute();
            }
            catch (Exception ex)
            {
                VerticesFailed.Add(vertex, ex);

                foreach (var dependent in vertex.Dependents.FlattenAndGetDistinct())
                    VerticesFailed.Add(dependent, new Exception("Dependency Failed"));

                return;
            }
            
            _verticesComplete.Add(vertex);

            foreach (var dependent in vertex.Dependents.Where(d=>d.Dependencies.All(n=>_verticesComplete.Contains(n))))
                Execute(dependent);
        }
    }
}
