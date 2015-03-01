namespace MPM.PDAG.UnitTests
{
    public class GraphCoordinatorEventRegister
    {
        public EventRegister GraphExecutiveStarted { get; private set; }

        internal GraphCoordinatorEventRegister(IGraphExecutionCoordinator graphExecutionCoordinator)
        {
            GraphExecutiveStarted = new EventRegister();

            graphExecutionCoordinator.OnGraphExecutiveStarted += delegate { GraphExecutiveStarted.Increment(); };
        }
    }
}
