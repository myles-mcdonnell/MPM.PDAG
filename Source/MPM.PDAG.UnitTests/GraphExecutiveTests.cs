using System;
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
        public void ConcurrencyThrottle_PostThreadSchedule()
        {
            ConcurrencyThrottle(ConcurrencyThrottleStrategy.PostThreadQueue);
        }

        [Test]
        public void ConcurrencyThrottle_PreThreadSchedule()
        {
            ConcurrencyThrottle(ConcurrencyThrottleStrategy.PreThreadQueue);
        }

        private static void ConcurrencyThrottle(ConcurrencyThrottleStrategy strategy)
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

            var executive = new ConcurrentGraphExecutive(new DirectedAcyclicGraph(node0), new ConcurrencyThrottle(2), strategy);

            var resetEvent = new ManualResetEventSlim();

            executive.OnGraphExecutionFinished += (sender, ergs) => resetEvent.Set();

            executive.Execute();

            resetEvent.Wait();

            Assert.IsTrue(maxCount.Max < 3);
        }
    }
}
