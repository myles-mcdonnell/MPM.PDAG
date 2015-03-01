using System.Linq;
using Moq;
using NUnit.Framework;

namespace MPM.PDAG.UnitTests
{
    [TestFixture]
    public class DirectedAcyclicGraphTests
    {
        [Test]
        public void AllVertices()
        {
            var vertex00Mock = new Mock<IVertex>();
            var vertex01Mock = new Mock<IVertex>();
            var vertex10Mock = new Mock<IVertex>();
            var vertex11Mock = new Mock<IVertex>();

            vertex00Mock.Setup(v => v.Dependencies).Returns(new IVertex[0]);
            vertex01Mock.Setup(v => v.Dependencies).Returns(new IVertex[0]);
            vertex00Mock.Setup(v => v.Dependents).Returns(new[] { vertex10Mock.Object, vertex11Mock.Object });
            vertex01Mock.Setup(v => v.Dependents).Returns(new[] { vertex10Mock.Object, vertex11Mock.Object });
            vertex10Mock.Setup(v => v.Dependencies).Returns(new[] {vertex00Mock.Object, vertex01Mock.Object});
            vertex11Mock.Setup(v => v.Dependencies).Returns(new[] { vertex00Mock.Object, vertex01Mock.Object });
            vertex10Mock.Setup(v => v.Dependents).Returns(new IVertex[0]);
            vertex11Mock.Setup(v => v.Dependents).Returns(new IVertex[0]);

            var vertices = new DirectedAcyclicGraph(vertex00Mock.Object, vertex01Mock.Object).AllVertices.ToList();

            Assert.AreEqual(4, vertices.Count());
            Assert.IsTrue(vertices.Contains(vertex00Mock.Object));
            Assert.IsTrue(vertices.Contains(vertex01Mock.Object));
            Assert.IsTrue(vertices.Contains(vertex10Mock.Object));
            Assert.IsTrue(vertices.Contains(vertex11Mock.Object));
        }

        [Test]
        public void RootVertices0()
        {
            var vertex00Mock = new Mock<IVertex>();
            var vertex01Mock = new Mock<IVertex>();
            var vertex10Mock = new Mock<IVertex>();
            var vertex11Mock = new Mock<IVertex>();

            vertex00Mock.Setup(v => v.Dependencies).Returns(new IVertex[0]);
            vertex01Mock.Setup(v => v.Dependencies).Returns(new IVertex[0]);
            vertex00Mock.Setup(v => v.Dependents).Returns(new[] { vertex10Mock.Object, vertex11Mock.Object });
            vertex01Mock.Setup(v => v.Dependents).Returns(new[] { vertex10Mock.Object, vertex11Mock.Object });
            vertex10Mock.Setup(v => v.Dependencies).Returns(new[] { vertex00Mock.Object, vertex01Mock.Object });
            vertex11Mock.Setup(v => v.Dependencies).Returns(new[] { vertex00Mock.Object, vertex01Mock.Object });
            vertex10Mock.Setup(v => v.Dependents).Returns(new IVertex[0]);
            vertex11Mock.Setup(v => v.Dependents).Returns(new IVertex[0]);

            var vertices = new DirectedAcyclicGraph(vertex00Mock.Object, vertex01Mock.Object).RootVertices.ToList();

            Assert.AreEqual(2, vertices.Count());
            Assert.IsTrue(vertices.Contains(vertex00Mock.Object));
            Assert.IsTrue(vertices.Contains(vertex01Mock.Object));
        }

        [Test]
        public void RootVertices1()
        {
            var vertex00Mock = new Mock<IVertex>();
            var vertex10Mock = new Mock<IVertex>();
            var vertex11Mock = new Mock<IVertex>();

            vertex00Mock.Setup(v => v.Dependencies).Returns(new IVertex[0]);
            vertex00Mock.Setup(v => v.Dependents).Returns(new[] { vertex10Mock.Object, vertex11Mock.Object });
            vertex10Mock.Setup(v => v.Dependencies).Returns(new[] { vertex00Mock.Object});
            vertex11Mock.Setup(v => v.Dependencies).Returns(new[] { vertex00Mock.Object });
            vertex10Mock.Setup(v => v.Dependents).Returns(new IVertex[0]);
            vertex11Mock.Setup(v => v.Dependents).Returns(new IVertex[0]);

            var vertices = new DirectedAcyclicGraph(vertex00Mock.Object, vertex10Mock.Object).RootVertices.ToList();

            Assert.AreEqual(1, vertices.Count());
            Assert.IsTrue(vertices.Contains(vertex00Mock.Object));
        }

        [Test]
        public void RootVertices2()
        {
            var vertex00Mock = new Mock<IVertex>();
            var vertex01Mock = new Mock<IVertex>();
            var vertex10Mock = new Mock<IVertex>();
            var vertex11Mock = new Mock<IVertex>();

            vertex00Mock.Setup(v => v.Dependencies).Returns(new IVertex[0]);
            vertex01Mock.Setup(v => v.Dependencies).Returns(new IVertex[0]);
            vertex00Mock.Setup(v => v.Dependents).Returns(new[] { vertex10Mock.Object, vertex11Mock.Object });
            vertex01Mock.Setup(v => v.Dependents).Returns(new[] { vertex10Mock.Object, vertex11Mock.Object });
            vertex10Mock.Setup(v => v.Dependencies).Returns(new[] { vertex00Mock.Object, vertex01Mock.Object });
            vertex11Mock.Setup(v => v.Dependencies).Returns(new[] { vertex00Mock.Object, vertex01Mock.Object });
            vertex10Mock.Setup(v => v.Dependents).Returns(new IVertex[0]);
            vertex11Mock.Setup(v => v.Dependents).Returns(new IVertex[0]);

            var vertices = new DirectedAcyclicGraph(vertex10Mock.Object, vertex11Mock.Object).RootVertices.ToList();

            Assert.AreEqual(2, vertices.Count());
            Assert.IsTrue(vertices.Contains(vertex10Mock.Object));
            Assert.IsTrue(vertices.Contains(vertex11Mock.Object));
        }

        [Test]
        public void TerminalVertices()
        {
            var vertex00Mock = new Mock<IVertex>();
            var vertex01Mock = new Mock<IVertex>();
            var vertex10Mock = new Mock<IVertex>();
            var vertex11Mock = new Mock<IVertex>();

            vertex00Mock.Setup(v => v.Dependencies).Returns(new IVertex[0]);
            vertex01Mock.Setup(v => v.Dependencies).Returns(new IVertex[0]);
            vertex00Mock.Setup(v => v.Dependents).Returns(new[] { vertex10Mock.Object, vertex11Mock.Object });
            vertex01Mock.Setup(v => v.Dependents).Returns(new[] { vertex10Mock.Object, vertex11Mock.Object });
            vertex10Mock.Setup(v => v.Dependencies).Returns(new[] { vertex00Mock.Object, vertex01Mock.Object });
            vertex11Mock.Setup(v => v.Dependencies).Returns(new[] { vertex00Mock.Object, vertex01Mock.Object });
            vertex10Mock.Setup(v => v.Dependents).Returns(new IVertex[0]);
            vertex11Mock.Setup(v => v.Dependents).Returns(new IVertex[0]);

            var vertices = new DirectedAcyclicGraph(vertex00Mock.Object, vertex01Mock.Object).TerminalVertices.ToList();

            Assert.AreEqual(2, vertices.Count());
            Assert.IsTrue(vertices.Contains(vertex10Mock.Object));
            Assert.IsTrue(vertices.Contains(vertex11Mock.Object));
        }
    }
}
