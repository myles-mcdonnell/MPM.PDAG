using System.Collections.Generic;
using System.Threading;
using Amib.Threading;
using NUnit.Framework;

namespace MPM.PDAG.UnitTests
{
    [TestFixture]
    public class ConcurrencyThrottleTests
    {
        [Test]
        public void ThrottleTest()
        {
            var throttle = new ConcurrencyThrottle{MaxValue = 5, Enabled = true};
            var maxReg = new MaxRegister();
            var threadPool = new SmartThreadPool();

            var state = new DoWorkState {Throttle = throttle, MaxRegister = maxReg};
            var workItemResults = new List<IWaitableResult>();
           
            for (var i = 0; i < 100; i++)
                workItemResults.Add(threadPool.QueueWorkItem(DoWork, state));

            SmartThreadPool.WaitAll(workItemResults.ToArray());

            Assert.IsTrue(maxReg.MaxValue <= 5);
        }

        public void DoWork(object state)
        {
            var doWorkState = (DoWorkState)state;

            doWorkState.Throttle.Enter();
            try
            {
                doWorkState.MaxRegister.Increment();

                Thread.Sleep(100);
            }
            finally
            {
                doWorkState.MaxRegister.Decrement();
                doWorkState.Throttle.Exit();
            }
        }
    }

    public class DoWorkState
    {
        public IConcurrencyThrottle Throttle { get; set; }
        public MaxRegister MaxRegister { get; set; }
    }
}
