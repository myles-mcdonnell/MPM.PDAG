namespace MPM.PDAG.UnitTests
{
    internal class VertexEventRegister
    {
        public EventRegister Cancelled { get; private set; }
        public EventRegister Completed { get; private set; }
        public EventRegister Failed { get; private set; }
        public EventRegister Started { get; private set; }

        internal VertexEventRegister(IVertex vertex)
        {
            Cancelled = new EventRegister();
            Completed = new EventRegister();
            Failed = new EventRegister();
            Started = new EventRegister();

            vertex.OnCancelled += delegate { Cancelled.Increment(); };
            vertex.OnCompleted += delegate { Completed.Increment(); };
            vertex.OnFailed += delegate { Failed.Increment(); };
            vertex.OnStarted += delegate { Started.Increment(); };
        }
    }
}
