using System.Linq;
using NUnit.Framework;
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
    }
}
