using System;
using System.Runtime.Remoting.Channels;
using System.Threading;

namespace MPM.PDAG.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create six node that each do nothing for at least 1 second
            var nodeA = new Vertex(ctx => Thread.Sleep(1000));
            var nodeB = new Vertex(ctx => Thread.Sleep(1000));
            var nodeC = new Vertex(ctx => Thread.Sleep(1000));
            var nodeD = new Vertex(ctx => Thread.Sleep(1000));
            var nodeE = new Vertex(ctx => Thread.Sleep(1000));
            var nodeF = new Vertex(ctx => Thread.Sleep(1000));

            //Create a DAG from the nodes
            nodeC.AddDependencies(nodeA, nodeB);
            nodeD.AddDependencies(nodeC);
            nodeE.AddDependencies(nodeC);
            nodeF.AddDependencies(nodeB);
            var graph = new DirectedAcyclicGraph(nodeA, nodeB, nodeC, nodeD, nodeE, nodeF);

            //Create a graph execution co-ordinator
            var graphExecutive = new GraphExecutive(graph);

            //set up reset event so we can have this thread wait for the graph execution to complete
            var resetEvent = new ManualResetEventSlim();
            Exception ex = null;
            graphExecutive.OnFailed += (sender, eargs) =>
            {
                resetEvent.Set();
                ex = eargs.Argument.Item2;
            };
            graphExecutive.OnFinished += (sender, eargs) => resetEvent.Set();

            //Execute the graph
            graphExecutive.Run();

            //Wait for execution to finish
            resetEvent.Wait();

            //Output result
            Console.WriteLine(ex != null ? ex.Message : "Graph Execution Complete");

#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}
