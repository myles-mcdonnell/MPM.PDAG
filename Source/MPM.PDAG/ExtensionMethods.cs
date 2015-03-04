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
using System.Linq;


#endregion

using System.Collections.Generic;

namespace MPM.PDAG
{
    public static class ExtensionMethods
    {
        public static IEnumerable<Vertex> FlattenAndGetDistinct(this IEnumerable<Vertex> vertices, bool upwards = false)
        {
            var set = new HashSet<Vertex>();
            var stack = new Stack<Vertex>();

			foreach (var vertex in vertices.ToArray())
            {
                stack.Push(vertex);
                while (stack.Count != 0)
                {
                    var current = stack.Pop();
                    if (set.Contains(current)) continue;
                    yield return current;
                    set.Add(current);
                    foreach (var child in upwards ? vertex.Dependencies : vertex.Dependents)
                        stack.Push(child);
                }
            }
        }
    }
}
