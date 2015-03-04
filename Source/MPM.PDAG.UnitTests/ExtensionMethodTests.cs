using System.Linq;
using Moq;
using NUnit.Framework;
using System.Threading;

namespace MPM.PDAG.UnitTests
{
    [TestFixture]
    public class ExtensionMethodTests
    {
       [Test]
        public void FlattenAndGetDistinctDownwards()
        {
			var vertex00 = new Vertex(()=>Thread.Sleep(1));
			var vertex01 = new Vertex(()=>Thread.Sleep(1));
			var vertex10 = new Vertex(()=>Thread.Sleep(1));
			var vertex11 = new Vertex(()=>Thread.Sleep(1));

			vertex00.AddDependencies (vertex10, vertex11);
			vertex01.AddDependencies (vertex10, vertex11);

			var vertices = new[] {vertex10, vertex11}.FlattenAndGetDistinct();

            Assert.AreEqual(4, vertices.Count());
            Assert.IsTrue(vertices.Contains(vertex00));
            Assert.IsTrue(vertices.Contains(vertex01));
            Assert.IsTrue(vertices.Contains(vertex10));
            Assert.IsTrue(vertices.Contains(vertex11));
        }

        [Test]
        public void FlattenAndGetDistinctUpwards()
        {
			var vertex00 = new Vertex(()=>Thread.Sleep(1));
			var vertex01 = new Vertex(()=>Thread.Sleep(1));
			var vertex10 = new Vertex(()=>Thread.Sleep(1));
			var vertex11 = new Vertex(()=>Thread.Sleep(1));

            vertex10.AddDependencies(vertex00, vertex01);
            vertex11.AddDependencies(vertex00, vertex01);

            var vertices = new[] { vertex10, vertex11 }.FlattenAndGetDistinct(true);

            Assert.AreEqual(4, vertices.Count());
            Assert.IsTrue(vertices.Contains(vertex00));
            Assert.IsTrue(vertices.Contains(vertex01));
            Assert.IsTrue(vertices.Contains(vertex10));
            Assert.IsTrue(vertices.Contains(vertex11));
         }
    }
}