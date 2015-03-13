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
