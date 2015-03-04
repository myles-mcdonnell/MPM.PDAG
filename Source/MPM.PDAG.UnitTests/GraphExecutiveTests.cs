//   Copyright 2012 Myles McDonnell (myles.mcdonnell.public@gmail.com)

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//     http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using MPM.PDAG.Impl;
using Moq;
using NUnit.Framework;
using log4net.Config;

#endregion

namespace MPM.PDAG.UnitTests
{
    [TestFixture]
    public class GraphExecutiveTests
    {
        [SetUp]
        public void SetUp()
        {
            BasicConfigurator.Configure();
        }

        [Test]
        public void ExecuteSimpleGraphToCompletion()
        {
            var sequenceRegister = new SequenceRegister();

            var node0 = new Mock<IVertex>();
            var node1 = new Mock<IVertex>();

            node0.Setup(n => n.Dependents).Returns(new []{node1.Object});
            node1.Setup(n => n.Dependencies).Returns(new[] {node0.Object});

            node0.Setup(n => n.Execute(It.IsAny<IGraphExecutive>())).Callback(() =>
            {
                sequenceRegister.Register(0);
                node0.Raise(n => n.OnCompleted += null, new EventArgs<IVertex>(node0.Object));
            });

            node1.Setup(n => n.Execute(It.IsAny<IGraphExecutive>())).Callback(() =>
            {
                sequenceRegister.Register(1);
                node1.Raise(n => n.OnCompleted += null, new EventArgs<IVertex>(node0.Object));
            });

            var graph = new Mock<IDirectedAcyclicGraph>();

            graph.Setup(g => g.AllVertices).Returns(new[] {node0.Object, node1.Object});
            graph.Setup(g => g.RootVertices).Returns(new[] { node0.Object });
            graph.Setup(g => g.TerminalVertices).Returns(new[] { node1.Object });

            var graphExecutive0 = (IGraphExecutive)new GraphExecutive(graph.Object);

            var eventReg = new GraphExecutiveEventRegister(graphExecutive0);

            graphExecutive0.Run();

            eventReg.ExecutiveFinishedEventCount.Wait(1, TimeSpan.FromSeconds(3));

            Assert.AreEqual(0, eventReg.ExecutiveFailedEventCount);
            Assert.AreEqual(0, eventReg.ExecutiveCancelledEventCount);
            Assert.AreEqual(1, eventReg.ExecutiveFinishedEventCount);
            Assert.AreEqual(0, eventReg.ExecutivePausedEventCount);
            Assert.AreEqual(0, eventReg.ExecutiveResumedEventCount);
            Assert.AreEqual(1, eventReg.ExecutiveStartedEventCount);
            Assert.IsTrue(sequenceRegister.CalledInSequence, "Nodes executed out of sequence");
            Assert.AreEqual(2, sequenceRegister.RegisterCount);
        }

        [Test]
        public void CancelSimpleGraphExecution()
        {
            var sequenceRegister = new SequenceRegister();
            
            var node0 = new Mock<IVertex>();
            var node1 = new Mock<IVertex>();
            var node2 = new Mock<IVertex>();

            node0.Setup(n => n.Dependencies).Returns(new IVertex[0]);
            node0.Setup(n => n.Dependents).Returns(new[] { node1.Object });
            node1.Setup(n => n.Dependencies).Returns(new[] { node0.Object });
            node1.Setup(n => n.Dependents).Returns(new[] { node2.Object });
            node2.Setup(n => n.Dependencies).Returns(new[] { node1.Object });
            node2.Setup(n => n.Dependents).Returns(new IVertex[0]);

            node0.Setup(n => n.Execute(It.IsAny<IGraphExecutive>())).Callback(() =>
            {
                node0.Setup(n => n.State).Returns(VertexState.Running);
                sequenceRegister.Register(0);
                node0.Setup(n => n.State).Returns(VertexState.Inactive);
                node0.Raise(n => n.OnCompleted += null, new EventArgs<IVertex>(node0.Object));
            });

            node1.Setup(n => n.Execute(It.IsAny<IGraphExecutive>())).Callback(delegate(IGraphExecutive graphExec)
            {
                node1.Setup(n => n.State).Returns(VertexState.Running);
                sequenceRegister.Register(1);
                node1.Setup(n => n.State).Returns(VertexState.Inactive);
                graphExec.Cancel();
                node1.Raise(n => n.OnCompleted += null, new EventArgs<IVertex>(node1.Object));
            });

            node2.Setup(n => n.Execute(It.IsAny<IGraphExecutive>())).Callback((IGraphExecutive graphExec) =>
            {
                node2.Setup(n => n.State).Returns(VertexState.Running);
                Assert.IsFalse(graphExec.CanVertexExecutionProceed());
                node2.Setup(n => n.State).Returns(VertexState.Inactive);
                node2.Raise(n => n.OnCompleted += null, new EventArgs<IVertex>(node2.Object));
            });

            var graph = new Mock<IDirectedAcyclicGraph>();

            graph.Setup(g => g.AllVertices).Returns(new[] { node0.Object, node1.Object, node2.Object });
            graph.Setup(g => g.RootVertices).Returns(new[] { node0.Object });
            graph.Setup(g => g.TerminalVertices).Returns(new[] { node2.Object });

            var graphExecutive = new GraphExecutive(graph.Object);

            var eventReg = new GraphExecutiveEventRegister(graphExecutive);

            graphExecutive.Run();

            eventReg.ExecutiveCancelledEventCount.Wait(1, TimeSpan.FromSeconds(3));

            Assert.AreEqual(0, eventReg.ExecutiveFailedEventCount);
            Assert.AreEqual(1, eventReg.ExecutiveCancelledEventCount);
            Assert.AreEqual(0, eventReg.ExecutiveFinishedEventCount);
            Assert.AreEqual(0, eventReg.ExecutivePausedEventCount);
            Assert.AreEqual(0, eventReg.ExecutiveResumedEventCount);
            Assert.AreEqual(1, eventReg.ExecutiveStartedEventCount);
            Assert.IsTrue(sequenceRegister.CalledInSequence, "Nodes executed out of sequence");
            Assert.AreEqual(2, sequenceRegister.RegisterCount);
        }

        [Test]
        [Ignore("needs fix")]
        public void PauseResumeSimpleGraphExecution()
        {
            var sequenceRegister = new SequenceRegister();
            IGraphExecutive graphExecutive = null;

            var node0 = new Vertex(state => sequenceRegister.Register(0));
            var node1 = new Vertex(state => { sequenceRegister.Register(1); graphExecutive.Pause(); });
            var node2 = new Vertex(state => sequenceRegister.Register(2));

            node2.AddDependencies(node1);
            node1.AddDependencies(node0);

            graphExecutive = new GraphExecutive(new DirectedAcyclicGraph(node0));

            var eventReg = new GraphExecutiveEventRegister(graphExecutive);

            graphExecutive.Run();

            EventRegister.WaitInSequence( TimeSpan.FromSeconds(3),eventReg.ExecutivePausePendingEventCount, eventReg.ExecutivePausedEventCount);

            graphExecutive.Resume();

            EventRegister.WaitInSequence(TimeSpan.FromSeconds(3), eventReg.ExecutiveResumePendingEventCount, eventReg.ExecutiveResumedEventCount, eventReg.ExecutiveFinishedEventCount);

            Assert.AreEqual(0, eventReg.ExecutiveFailedEventCount);
            Assert.AreEqual(0, eventReg.ExecutiveCancelledEventCount);
            Assert.AreEqual(1, eventReg.ExecutiveFinishedEventCount);
            Assert.AreEqual(1, eventReg.ExecutivePausedEventCount);
            Assert.AreEqual(1, eventReg.ExecutiveResumedEventCount);
            Assert.AreEqual(1, eventReg.ExecutiveStartedEventCount);
            Assert.IsTrue(sequenceRegister.CalledInSequence, "Nodes executed out of sequence");
            Assert.AreEqual(3, sequenceRegister.RegisterCount);
        }

        [Test]
        [Ignore("needs fix")]
        public void PauseCancelSimpleGraphExecution()
        {
            var sequenceRegister = new SequenceRegister();
            IGraphExecutive graphExecutive = null;

            var node0 = new Vertex(state => sequenceRegister.Register(0));
            var node1 = new Vertex(state => { sequenceRegister.Register(1); graphExecutive.Pause(); });
            var node2 = new Vertex(state => sequenceRegister.Register(2));

            node2.AddDependencies(node1);
            node1.AddDependencies(node0);

            graphExecutive = new GraphExecutive(new DirectedAcyclicGraph(node0));

            var eventReg = new GraphExecutiveEventRegister(graphExecutive);

            graphExecutive.Run();

            EventRegister.WaitInSequence(TimeSpan.FromSeconds(3), eventReg.ExecutivePausePendingEventCount, eventReg.ExecutivePausedEventCount);

            graphExecutive.Cancel();

            EventRegister.WaitInSequence(TimeSpan.FromSeconds(3), eventReg.ExecutiveCancelPendingEventCount, eventReg.ExecutiveCancelledEventCount);

            Assert.AreEqual(0, eventReg.ExecutiveFailedEventCount);
            Assert.AreEqual(1, eventReg.ExecutiveCancelledEventCount);
            Assert.AreEqual(0, eventReg.ExecutiveFinishedEventCount);
            Assert.AreEqual(1, eventReg.ExecutivePausedEventCount);
            Assert.AreEqual(0, eventReg.ExecutiveResumedEventCount);
            Assert.AreEqual(1, eventReg.ExecutiveStartedEventCount);
            Assert.IsTrue(sequenceRegister.CalledInSequence, "Nodes executed out of sequence");
            Assert.AreEqual(2, sequenceRegister.RegisterCount);
        }

        [Test]
        [Ignore("needs fix")]
        public void SimpleGraphExecutionFailure()
        {
            var sequenceRegister = new SequenceRegister();

            var node0 = new Vertex(state => sequenceRegister.Register(0));
            var node1 = new Vertex(state => { sequenceRegister.Register(1); throw new Exception(); });
            var node2 = new Vertex(state => sequenceRegister.Register(2));

            node2.AddDependencies(node1);
            node1.AddDependencies(node0);

            var graphExecutive = new GraphExecutive(new DirectedAcyclicGraph(node0));

            var eventReg = new GraphExecutiveEventRegister(graphExecutive);

            graphExecutive.Run();

            eventReg.ExecutiveFailedEventCount.Wait(1, TimeSpan.FromSeconds(3));

            Assert.AreEqual(1, eventReg.ExecutiveFailedEventCount);
            Assert.AreEqual(0, eventReg.ExecutiveCancelledEventCount);
            Assert.AreEqual(0, eventReg.ExecutiveFinishedEventCount);
            Assert.AreEqual(0, eventReg.ExecutivePausedEventCount);
            Assert.AreEqual(0, eventReg.ExecutiveResumedEventCount);
            Assert.AreEqual(1, eventReg.ExecutiveStartedEventCount);
            Assert.IsTrue(sequenceRegister.CalledInSequence, "Nodes executed out of sequence");
            Assert.AreEqual(2, sequenceRegister.RegisterCount);
        }
    }
}