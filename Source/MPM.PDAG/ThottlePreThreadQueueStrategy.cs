namespace MPM.PDAG
{
    internal class ThottlePreThreadQueueStrategy : IConcurrencyThottleStrategy
    {
        private readonly ConcurrencyThrottle _throttle;

        internal ThottlePreThreadQueueStrategy(ConcurrencyThrottle throttle)
        {
            _throttle = throttle;
        }

        public void EnterPreThreadQueue()
        {
            _throttle.Enter();
        }

        public void EnterPostThreadQueue()
        {
           
        }

        public void Exit()
        {
            _throttle.Exit();
        }
    }
}
