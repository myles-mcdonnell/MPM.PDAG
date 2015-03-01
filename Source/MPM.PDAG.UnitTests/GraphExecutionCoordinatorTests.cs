using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using Moq;
using NUnit.Framework;

namespace MPM.PDAG.UnitTests
{
    [TestFixture]
    public class GraphExecutionCoordinatorTests
    {
        //[Test]
        //public void ExplicitlyExecuteEntireGraph()
        //{
        //    var graph = new Mock<IDirectedAcyclicGraph>();
        //    var graphExecutiveFactory = new Mock<IGraphExecutiveFactory>();
            
        //    graphExecutiveFactory.Setup(g => g.Build(graph.Object)).Returns(delegate(IDirectedAcyclicGraph dag)
        //                                                                        {
        //                                                                            var graphExecutive = new Mock<IGraphExecutive>();
        //                                                                            graphExecutive.Setup(g => g.Graph).Returns(dag);
        //                                                                            return graphExecutive.Object;
        //                                                                        });

        //    var coordinator = (IGraphExecutionCoordinator)new GraphExecutionCoordinator(graph.Object,graphExecutiveFactory.Object);

        //    coordinator.OnGraphExecutiveStarted += delegate(object sender, EventArgs<IGraphExecutive> args) { Assert.IsNotNull(args.Argument);Assert.AreEqual(args.Argument.Graph, graph.Object); };
           
        //    var eventRegister = new GraphCoordinatorEventRegister(coordinator);

        //    coordinator.ExecuteAll();

        //    eventRegister.GraphExecutiveStarted.Wait(1, TimeSpan.FromSeconds(2));
        //}

        [Test]
        public void ReactivelyExecuteEntireGraph()
        {
            var vertex = new Mock<IVertex>();
            var vertexGraph = new Mock<IDirectedAcyclicGraph>();
            var graph = new Mock<IDirectedAcyclicGraph>();
            var graphExecutiveFactory = new Mock<IGraphExecutiveFactory>();
            
            graph.Setup(g => g.AllVertices).Returns(new[]{vertex.Object});
            vertexGraph.Setup(g => g.AllVertices).Returns(new[] { vertex.Object });
            vertex.Setup(g => g.GetGraph()).Returns(vertexGraph.Object);

            graphExecutiveFactory.Setup(g => g.Build(vertexGraph.Object)).Returns(delegate(IDirectedAcyclicGraph dag)
                                                                                {
                                                                                    var graphExecutive = new Mock<IGraphExecutive>();
                                                                                    graphExecutive.Setup(g => g.Graph).Returns(dag);
                                                                                    return graphExecutive.Object;
                                                                                });

            var coordinator = (IGraphExecutionCoordinator)new GraphExecutionCoordinator(graph.Object, graphExecutiveFactory.Object);

            coordinator.OnGraphExecutiveStarted += delegate(object sender, EventArgs<IGraphExecutive> args) { Assert.IsNotNull(args.Argument); Assert.AreEqual(args.Argument.Graph, vertexGraph.Object); };

            var eventRegister = new GraphCoordinatorEventRegister(coordinator);

            vertex.Raise(v => v.OnRequiresReExecution += null, new EventArgs<IVertex>(vertex.Object));

            eventRegister.GraphExecutiveStarted.Wait(1, TimeSpan.FromSeconds(2));
        }
    }
}
