namespace MPM.PDAG.UnitTests
{
    public class SequenceRegister
    {
        private readonly object _lock = new object();
        private bool _outOfSequence;
        private int _last = int.MinValue;

        public void Register(int i)
        {
            lock (_lock)
            {
                _outOfSequence = i <= _last;
                _last = i;
                RegisterCount++;
            }
        }

        public bool CalledInSequence { get { return !_outOfSequence; } }

        public int RegisterCount { get; private set; }
    }
}
