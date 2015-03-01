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
using System.Threading;
using Moq;
using NUnit.Framework;

#endregion

namespace MPM.PDAG.UnitTests
{
    [TestFixture]
    public class VertexTests
    {
        [Test]
        [ExpectedException(typeof (CircularDependencyException))]
        public void CircularDependencyTest()
        {
            var node = new Vertex();

            node.AddDependencies(node);
        }

        [Test]
        [ExpectedException(typeof (CircularDependencyException))]
        public void NestedCircurlarDependencyTest()
        {
            var node = new Vertex();

            var node2 = new Vertex();
            var node3 = new Vertex();
            var node4 = new Vertex();

            node2.AddDependencies(node);
            node3.AddDependencies(node2);
            node4.AddDependencies(node3);

            node.AddDependencies(node2);
        }

        [Test]
        public void ExecuteSingleVertexSuccess()
        {
            var vertex = new Vertex();

            var vertexRegister = new VertexEventRegister(vertex);

            var graphExecutiveMock = new Mock<IGraphExecutive>();
            graphExecutiveMock.Setup(g => g.CanVertexExecutionProceed()).Returns(true);

            Assert.AreEqual(ExecutionResult.None, vertex.LastExecutionResult);
            vertex.Execute(graphExecutiveMock.Object);

            vertexRegister.Completed.Wait(1, TimeSpan.FromSeconds(3));

            Assert.AreEqual(ExecutionResult.Success, vertex.LastExecutionResult);
            Assert.AreEqual(0, vertexRegister.Cancelled);
            Assert.AreEqual(0, vertexRegister.Failed);
            Assert.AreEqual(1, vertexRegister.Completed);
            Assert.AreEqual(1, vertexRegister.Started);
        }

        [Test]
        public void ExecuteSingleVertexFailure()
        {
            var vertex = new Vertex(state => { throw new Exception(); });

            var vertexRegister = new VertexEventRegister(vertex);

            var graphExecutiveMock = new Mock<IGraphExecutive>();
            graphExecutiveMock.Setup(g => g.CanVertexExecutionProceed()).Returns(true);

            Assert.AreEqual(ExecutionResult.None, vertex.LastExecutionResult);
            vertex.Execute(graphExecutiveMock.Object);

            vertexRegister.Failed.Wait(1,TimeSpan.FromSeconds(3));
            Assert.AreEqual(ExecutionResult.Fail, vertex.LastExecutionResult);
            Assert.AreEqual(0, vertexRegister.Cancelled);
            Assert.AreEqual(1, vertexRegister.Failed);
            Assert.AreEqual(0, vertexRegister.Completed);
            Assert.AreEqual(1, vertexRegister.Started);
        }

        [Test]
        public void ExecuteSingleVertexCancel()
        {
            var vertex = new Vertex(state =>
                                        {
                                            var started = DateTime.Now;
                                            while (!state.CancellationToken.IsCancellationRequested)
                                            {
                                                Thread.Sleep(100);

                                                if (DateTime.Now-started>TimeSpan.FromSeconds(5))
                                                    throw new TimeoutException();
                                            }

                                            state.CancellationToken.ThrowIfCancellationRequested();
                                        });

            var vertexRegister = new VertexEventRegister(vertex);

            vertex.OnStarted += delegate { vertex.Cancel(); };

            var graphExecutiveMock = new Mock<IGraphExecutive>();
            graphExecutiveMock.Setup(g => g.CanVertexExecutionProceed()).Returns(true);

            Assert.AreEqual(ExecutionResult.None, vertex.LastExecutionResult);
            vertex.Execute(graphExecutiveMock.Object);

            vertexRegister.Cancelled.Wait(1, TimeSpan.FromSeconds(3));

            Assert.AreEqual(ExecutionResult.Cancel, vertex.LastExecutionResult);
            Assert.AreEqual(1, vertexRegister.Cancelled);
            Assert.AreEqual(0, vertexRegister.Failed);
            Assert.AreEqual(0, vertexRegister.Completed);
            Assert.AreEqual(1, vertexRegister.Started);
        }
    }
}