using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;

namespace MPM.PDAG.UnitTests
{
    [TestFixture]
    public class GraphExecutiveTests
    {
        [Test]
        public void SimpleGraphExecute_NoConcurrency()
        {
            var stack = new Stack<int>();

            var node0 = new Vertex(() => stack.Push(1));
            var node1 = new Vertex(() => stack.Push(2));
            var node2 = new Vertex(() => stack.Push(3));

            node2.AddDependencies(node1);
            node1.AddDependencies(node0);

            var executive = new GraphExecutive(new DirectedAcyclicGraph(node0, node1, node2));
            
            var resetEvent = new ManualResetEventSlim();

            executive.OnGraphExecutionFinished+=(sender, ergs)=>resetEvent.Set();

            executive.Execute();

            resetEvent.Wait();

            for (var i = 3; i > 0; i--)
                Assert.AreEqual(i, stack.Pop());
        }

        [Test]
        public void SimpleGraphExecute_WithConcurrency_NoThrottle()
        {
            //TODO: Sledgehammer, should be improved
            for (int ii = 0; ii < 1000; ii++)
            {
                var stack = new ConcurrentStack<int>();

                var node0 = new Vertex(() => stack.Push(1));
                var node1 = new Vertex(() => stack.Push(2));
                var node2 = new Vertex(() => stack.Push(2));
                var node3 = new Vertex(() => stack.Push(2));
                var node4 = new Vertex(() => stack.Push(3));

                node3.AddDependencies(node0);
                node2.AddDependencies(node0);
                node1.AddDependencies(node0);

                node4.AddDependencies(node1, node2, node3);

                var executive = new GraphExecutive(new DirectedAcyclicGraph(node0, node1, node2, node3, node4));

                executive.ExecuteAndWait();

                Assert.IsTrue(executive.VerticesFailed.Count==0);

                var vals = stack.ToArray();

                var expected = new[] {3, 2, 2, 2, 1};

                for (var i = 0; i < expected.Length; i++)
                    Assert.AreEqual(expected[i], vals[i]);
            }
        }

        public class MaxCount
        {
            private readonly object _lock = new object();
            private int _current;

            public void Inc()
            {
                lock (_lock)
                {
                    _current++;
                    Max = Math.Max(Max, _current);
                }
            }

            public void Dec()
            {
                lock (_lock)
                {
                    _current--;
                }
            }

            public int Max { get; private set; }
        }

        [Test]
        public void ConcurrencyThrottle()
        {
            var maxCount = new MaxCount();

            var node0 = new Vertex(() => Thread.Sleep(100));
            var node10 = new Vertex(() =>
            {
                maxCount.Inc();
                Thread.Sleep(1000);
                maxCount.Dec();
            });
            var node11 = new Vertex(() =>
            {
                maxCount.Inc();
                Thread.Sleep(1000);
                maxCount.Dec();
            });
            var node12 = new Vertex(() =>
            {
                maxCount.Inc();
                Thread.Sleep(1000);
                maxCount.Dec();
            });
            var node13 = new Vertex(() =>
            {
                maxCount.Inc();
                Thread.Sleep(1000);
                maxCount.Dec();
            });
            var node14 = new Vertex(() =>
            {
                maxCount.Inc();
                Thread.Sleep(1000);
                maxCount.Dec();
            });
            var node15 = new Vertex(() =>
            {
                maxCount.Inc();
                Thread.Sleep(1000);
                maxCount.Dec();
            });
            var node16 = new Vertex(() =>
            {
                maxCount.Inc();
                Thread.Sleep(1000);
                maxCount.Dec();
            });

            node10.AddDependencies(node0);
            node11.AddDependencies(node0);
            node12.AddDependencies(node0);
            node13.AddDependencies(node0);
            node14.AddDependencies(node0);
            node15.AddDependencies(node0);
            node16.AddDependencies(node0);

            var executive = new GraphExecutive(new DirectedAcyclicGraph(node0), new ConcurrencyThrottle(2));

            var resetEvent = new ManualResetEventSlim();

            executive.OnGraphExecutionFinished += (sender, ergs) => resetEvent.Set();

            executive.Execute();

            resetEvent.Wait();

            Assert.IsTrue(maxCount.Max < 3);
        }
    }
}
