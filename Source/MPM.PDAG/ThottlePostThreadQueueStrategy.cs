namespace MPM.PDAG
{
    internal class ThottlePostThreadQueueStrategy : IConcurrencyThottleStrategy
    {
        private readonly ConcurrencyThrottle _throttle;

        internal ThottlePostThreadQueueStrategy(ConcurrencyThrottle throttle)
        {
            _throttle = throttle;
        }

        public void EnterPreThreadQueue()
        {
        }

        public void EnterPostThreadQueue()
        {
            _throttle.Enter();
        }

        public void Exit()
        {
            _throttle.Exit();
        }
    }
}
