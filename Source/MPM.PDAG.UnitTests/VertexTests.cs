//   Copyright 2012 Myles McDonnell (myles.mcdonnell.public@gmail.com)

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//     http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.


using System.Threading;
using System.Linq;
using NUnit.Framework;

namespace MPM.PDAG.UnitTests
{
    [TestFixture]
    public class VertexTests
    {
        [Test]
        [ExpectedException(typeof (CircularDependencyException))]
        public void CircularDependencyTest()
        {
			var node = new Vertex(()=>Thread.Sleep(1));

            node.AddDependencies(node);
        }

        [Test]
        [ExpectedException(typeof (CircularDependencyException))]
        public void NestedCircularDependencyTest()
        {
			var node = new Vertex(()=>Thread.Sleep(1));

			var node2 = new Vertex(()=>Thread.Sleep(1));
			var node3 = new Vertex(()=>Thread.Sleep(1));
			var node4 = new Vertex(()=>Thread.Sleep(1));

            node2.AddDependencies(node);
            node3.AddDependencies(node2);
            node4.AddDependencies(node3);

            node.AddDependencies(node2);
        }

		[Test]
		public void RemoveRedundantDependencies()
		{
			var node = new Vertex(()=>Thread.Sleep(1));

			var node2 = new Vertex(()=>Thread.Sleep(1));
			var node3 = new Vertex(()=>Thread.Sleep(1));

			node3.AddDependencies (node2, node);

			Assert.AreEqual (2, node3.Dependencies.Count());

			node3.RemoveRedundantDependencies ();

			Assert.AreEqual (2, node3.Dependencies.Count());

			node2.AddDependencies (node);

			node3.RemoveRedundantDependencies ();

			Assert.AreEqual (1, node3.Dependencies.Count());
		}

		[Test]
		public void DoNotAddRedundantDependency()
		{
			var node = new Vertex(()=>Thread.Sleep(1));

			var node2 = new Vertex(()=>Thread.Sleep(1));
			var node3 = new Vertex(()=>Thread.Sleep(1));

			node2.AddDependencies (node);
			node3.AddDependencies (node2);

			Assert.AreEqual (1, node3.Dependencies.Count());

			node3.AddDependencies (node);

			Assert.AreEqual (1, node3.Dependencies.Count());
		}
    }
}