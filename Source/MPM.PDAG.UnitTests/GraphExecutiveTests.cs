using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace MPM.PDAG.UnitTests
{
    [TestFixture]
    public class GraphExecutiveTests
    {
        [Test]
        public void SimpleGraphExecuteSequentially()
        {
            var stack = new Stack<int>();

            var node0 = new Vertex(() => stack.Push(1));
            var node1 = new Vertex(() => stack.Push(2));
            var node2 = new Vertex(() => stack.Push(3));

            node2.AddDependencies(node1);
            node1.AddDependencies(node0);

            new SequentialGraphExecutive(new DirectedAcyclicGraph(node0, node1, node2)).Execute();

            for (var i = 3; i > 0; i--)
                Assert.AreEqual(i, stack.Pop());
        }

        [Test]
        public void SimpleGraphExecuteConcurrently()
        {
            var stack = new Stack<int>();

            var node0 = new Vertex(() => stack.Push(1));
            var node1 = new Vertex(() => stack.Push(2));
            var node2 = new Vertex(() => stack.Push(3));

            node2.AddDependencies(node1);
            node1.AddDependencies(node0);

            var executive = new ConcurrentGraphExecutive(new DirectedAcyclicGraph(node0, node1, node2));
            
            var resetEvent = new ManualResetEventSlim();

            executive.OnGraphExecutionFinished+=(sender, ergs)=>resetEvent.Set();

            executive.Execute();

            resetEvent.Wait();

            for (var i = 3; i > 0; i--)
                Assert.AreEqual(i, stack.Pop());
        }
    }
}
