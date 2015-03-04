using System.Linq;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading;

namespace MPM.PDAG.UnitTests
{
    [TestFixture]
    public class DirectedAcyclicGraphTests
    {
        [Test]
        public void SimpleGraphAggregates()
        {
			var node0 = new Vertex (() => Thread.Sleep(1));
			var node1 = new Vertex (() => Thread.Sleep(1));
			var node2 = new Vertex (() => Thread.Sleep(1));

			node2.AddDependencies (node1);
			node1.AddDependencies (node0);

			var graph = new DirectedAcyclicGraph (node0, node1, node2);

            Assert.AreEqual(3, graph.AllVertices.Count());
			Assert.AreEqual(1, graph.RootVertices.Count());
			Assert.AreEqual(1, graph.TerminalVertices.Count());

			Assert.AreEqual (node0, graph.RootVertices.First ());
			Assert.AreEqual (node2, graph.TerminalVertices.First ());

        }
			
		[Test]
		public void SimpleGraphExecuteSequentially()
		{
			var stack = new Stack<int> ();

			var node0 = new Vertex (() => stack.Push (1));
			var node1 = new Vertex (() => stack.Push (2));
			var node2 = new Vertex (() => stack.Push (3));

			node2.AddDependencies (node1);
			node1.AddDependencies (node0);

			new DirectedAcyclicGraph (node0, node1, node2).ExecuteSequentially ();

			for (int i = 3; i > 0; i--) {
				Assert.AreEqual (i, stack.Pop ());
			}
		}
    }
}
