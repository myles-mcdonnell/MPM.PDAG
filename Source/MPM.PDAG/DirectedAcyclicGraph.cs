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

using System.Collections.Generic;
using System.Linq;

namespace MPM.PDAG
{
    public class DirectedAcyclicGraph 
    {
        public DirectedAcyclicGraph(params Vertex[] vertices) :
            this((IEnumerable<Vertex>)vertices) {}

        public DirectedAcyclicGraph(IEnumerable<Vertex> vertices)
        {
            AllVertices = vertices.FlattenAndGetDistinct().ToList();

            RootVertices =
                AllVertices.Where(v => !v.Dependencies.Any() || v.Dependencies.Count(d => AllVertices.Contains(d)) == 0)
                    .ToList();

            TerminalVertices = AllVertices.Where(v => !v.Dependents.Any()).ToList();

            foreach (var terminalVertex in TerminalVertices)
                terminalVertex.RemoveRedundantDependencies();
        }

        public IEnumerable<Vertex> AllVertices { get; private set; }
        public IEnumerable<Vertex> RootVertices { get; private set; }
        public IEnumerable<Vertex> TerminalVertices { get; private set; }

		public void ExecuteSequentially()
		{
			var verticesComplete = new List<Vertex>();
			var allVertices = AllVertices.ToList();
			var pendingVertices = RootVertices.ToList();

			do{
				foreach(var vertex in pendingVertices.ToList()){
					vertex.Execute();
					allVertices.Remove(vertex);
					verticesComplete.Add(vertex);
				}

				pendingVertices = allVertices.Where(v=>v.Dependencies.All(d=>verticesComplete.Contains(d))).ToList();
			}
			while(pendingVertices.Count()>0);
		}
    }
}
