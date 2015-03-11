namespace MPM.PDAG
{
    internal interface IConcurrencyThottleStrategy
    {
        void EnterPreThreadQueue();
        void EnterPostThreadQueue();
        void Exit();
    }
}