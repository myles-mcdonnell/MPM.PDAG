using System;

namespace MPM.PDAG
{
    internal class ConcurrencyThrottleStrategyFactory
    {
        private readonly ConcurrencyThrottle _throttle;

        internal ConcurrencyThrottleStrategyFactory(ConcurrencyThrottle throttle)
        {
            _throttle = throttle;
        }

        internal IConcurrencyThottleStrategy Build(ConcurrencyThrottleStrategy strategy)
        {
            switch (strategy)
            {
                case ConcurrencyThrottleStrategy.PostThreadQueue:
                    return new ThottlePostThreadQueueStrategy(_throttle);

                case ConcurrencyThrottleStrategy.PreThreadQueue:
                    return new ThottlePreThreadQueueStrategy(_throttle);

                default:
                    throw new Exception("Unrecognised ConcurrencyThrottleStrategy : " + strategy);
            }
        }
    }
}
