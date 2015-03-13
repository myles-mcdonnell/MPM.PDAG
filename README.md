# MPM.PDAG
A component library for building directed acyclic graphs and executing the vertices concurrently.

Imagine a graph like so..

<img src="https://db.tt/0yiN7MgI"/>

..we can reason that A and B can be executed concurrently. C can be executed when A & B are complete, F can be executed when only B is complete and D & E can be excuted concurrently when C is complete. F may be executed concurrently with and/or A | C | D | E.

MPM.PDAG provides a mechanism for building directed acyclic graphs to any level of complexity and will automatically determine the maximum level 
of concurrency when executing the graph.  When building a graph an exception will be thrown if a circular reference is attempted and a concurrency throttle may
be provided when executing a graph.  What action is performed when a vertex is executed is specified by passing an action to the vertex constructor.

Although this code has been kicking around in my tool box for a few years only the simple use cases are complete.  There is work to be done around graph execution cancellation and a few other features, PR's are more than welcome.

<pre>
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
var graphExecutive = new ConcurrentGraphExecutive(graph, new ConcurrencyThrottle(8), ConcurrencyThrottleStrategy.PreThreadQueue);

graphExecutive.ExecuteAndWait();
            
//Output result
Console.WriteLine("Graph Execution Complete");
Console.WriteLine(graphExecutive.VerticesFailed.Any() ? "\tn nodes failed" : "\tall nodes successful");
</pre>