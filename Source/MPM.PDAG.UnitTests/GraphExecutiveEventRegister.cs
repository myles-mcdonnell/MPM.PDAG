namespace MPM.PDAG.UnitTests
{
    public class GraphExecutiveEventRegister
    {
        public EventRegister ExecutiveResumedEventCount { get; private set; }
        public EventRegister ExecutivePausedEventCount { get; private set; }
        public EventRegister ExecutiveFinishedEventCount { get; private set; }
        public EventRegister ExecutiveStartedEventCount { get; private set; }
        public EventRegister ExecutiveCancelledEventCount { get; private set; }
        public EventRegister VertexProgressEventCount { get; private set; }
        public EventRegister ExecutivePausePendingEventCount { get; private set; }
        public EventRegister ExecutiveCancelPendingEventCount { get; private set; }
        public EventRegister ExecutiveResumePendingEventCount { get; private set; }
        public EventRegister ExecutiveFailedEventCount { get; private set; }

        public GraphExecutiveEventRegister(IGraphExecutive graphExecutive)
        {
            ExecutiveResumedEventCount = new EventRegister();
            ExecutivePausedEventCount = new EventRegister();
            ExecutiveFinishedEventCount = new EventRegister();
            ExecutiveStartedEventCount = new EventRegister();
            ExecutiveCancelledEventCount = new EventRegister();
            ExecutivePausePendingEventCount = new EventRegister();
            ExecutiveCancelPendingEventCount = new EventRegister();
            ExecutiveResumePendingEventCount = new EventRegister();
            ExecutiveFailedEventCount = new EventRegister();
       
            graphExecutive.OnFinished += (sender, e) => ExecutiveFinishedEventCount.Increment();
            graphExecutive.OnStarted += (sender, e) => ExecutiveStartedEventCount.Increment();
            graphExecutive.OnResumed += (sender, e) => ExecutiveResumedEventCount.Increment();
            graphExecutive.OnPaused += (sender, e) => ExecutivePausedEventCount.Increment();
            graphExecutive.OnCancelled += (sender, e) => ExecutiveCancelledEventCount.Increment();
         //   graphExecutive.OnVertexProgress += (sender, e) => VertexProgressEventCount.Increment();
            graphExecutive.OnPausePending += (sender, e) => ExecutivePausePendingEventCount.Increment();
            graphExecutive.OnCancelPending += (sender, e) => ExecutiveCancelPendingEventCount.Increment();
            graphExecutive.OnResumePending += (sender, e) => ExecutiveResumePendingEventCount.Increment();
            graphExecutive.OnFailed += (sender, e) => ExecutiveFailedEventCount.Increment();
        }
    }
}
