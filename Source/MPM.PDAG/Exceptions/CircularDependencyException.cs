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

#endregion

namespace MPM.PDAG.Exceptions
{
    /// <summary>
    ///   Thrown if the current operation would result in a circular node dependency
    /// </summary>
    public class CircularDependencyException : Exception
    {
        private readonly Vertex _vertex;

        public CircularDependencyException(Vertex vertex)
        {
            _vertex = vertex;
        }

        public Vertex Vertex
        {
            get { return _vertex; }
        }
    }
}