using System;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Threading;

namespace MPM.PDAG.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            //Create six node that each do nothing for at least 1 second
            var nodeA = new Vertex(() => Thread.Sleep(1000));
            var nodeB = new Vertex(() => Thread.Sleep(1000));
            var nodeC = new Vertex(() => Thread.Sleep(1000));
            var nodeD = new Vertex(() => Thread.Sleep(1000));
            var nodeE = new Vertex(() => Thread.Sleep(1000));
            var nodeF = new Vertex(() => Thread.Sleep(1000));

            //Create a DAG from the nodes
            nodeC.AddDependencies(nodeA, nodeB);
            nodeD.AddDependencies(nodeC);
            nodeE.AddDependencies(nodeC);
            nodeF.AddDependencies(nodeB);
            var graph = new DirectedAcyclicGraph(nodeA, nodeB, nodeC, nodeD, nodeE, nodeF);

            //Create a graph executive
            var graphExecutive = new GraphExecutive(graph, new ConcurrencyThrottle(8));

            graphExecutive.ExecuteAndWait();
            
            //Output result
            Console.WriteLine("Graph Execution Complete");
            Console.WriteLine(graphExecutive.VerticesFailed.Any() ? "\tn nodes failed" : "\tall nodes successful");

#if DEBUG
            Console.ReadLine();
#endif
        }
    }
}
