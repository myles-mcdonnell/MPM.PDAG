using System.Linq;
using Moq;
using NUnit.Framework;

namespace MPM.PDAG.UnitTests
{
    [TestFixture]
    public class ExtensionMethodTests
    {
        [Test]
        public void FlattenAndGetDistinctDownwards()
        {
            var vertex00Mock = new Mock<IVertex>();
            var vertex01Mock = new Mock<IVertex>();
            var vertex10Mock = new Mock<IVertex>();
            var vertex11Mock = new Mock<IVertex>();

            vertex00Mock.Setup(v => v.Dependents).Returns(new[] {vertex10Mock.Object, vertex11Mock.Object});
            vertex01Mock.Setup(v => v.Dependents).Returns(new[] {vertex10Mock.Object, vertex11Mock.Object});

            var vertices = new[] {vertex00Mock.Object, vertex01Mock.Object}.FlattenAndGetDistinct();

            Assert.AreEqual(4, vertices.Count());
            Assert.IsTrue(vertices.Contains(vertex00Mock.Object));
            Assert.IsTrue(vertices.Contains(vertex01Mock.Object));
            Assert.IsTrue(vertices.Contains(vertex10Mock.Object));
            Assert.IsTrue(vertices.Contains(vertex11Mock.Object));

            vertex00Mock.VerifyAll();
            vertex01Mock.VerifyAll();
        }

        [Test]
        public void FlattenAndGetDistinctUpwards()
        {
            var vertex00Mock = new Mock<IVertex>();
            var vertex01Mock = new Mock<IVertex>();
            var vertex10Mock = new Mock<IVertex>();
            var vertex11Mock = new Mock<IVertex>();

            vertex10Mock.Setup(v => v.Dependencies).Returns(new[] { vertex00Mock.Object, vertex01Mock.Object });
            vertex11Mock.Setup(v => v.Dependencies).Returns(new[] { vertex00Mock.Object, vertex01Mock.Object });

            var vertices = new[] { vertex10Mock.Object, vertex11Mock.Object }.FlattenAndGetDistinct(true);

            Assert.AreEqual(4, vertices.Count());
            Assert.IsTrue(vertices.Contains(vertex00Mock.Object));
            Assert.IsTrue(vertices.Contains(vertex01Mock.Object));
            Assert.IsTrue(vertices.Contains(vertex10Mock.Object));
            Assert.IsTrue(vertices.Contains(vertex11Mock.Object));

            vertex00Mock.VerifyAll();
            vertex01Mock.VerifyAll();
        }
    }
}