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
    public class DirectedAcyclicGraph : IDirectedAcyclicGraph
    {
        public DirectedAcyclicGraph(params IVertex[] vertices) :
            this((IEnumerable<IVertex>)vertices) {}

        public DirectedAcyclicGraph(IEnumerable<IVertex> vertices)
        {
            AllVertices = vertices.FlattenAndGetDistinct().ToList();

            RootVertices =
                AllVertices.Where(v => !v.Dependencies.Any() || v.Dependencies.Count(d => AllVertices.Contains(d)) == 0)
                    .ToList();

            TerminalVertices = AllVertices.Where(v => !v.Dependents.Any()).ToList();

            //TODO 
            //foreach (var terminalVertex in TerminalVertices)
            //    terminalVertex.RemoveRedundantDependencies();
        }

        public IEnumerable<IVertex> AllVertices { get; private set; }
        public IEnumerable<IVertex> RootVertices { get; private set; }
        public IEnumerable<IVertex> TerminalVertices { get; private set; }
    }
}
