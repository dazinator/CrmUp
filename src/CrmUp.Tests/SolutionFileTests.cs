using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework;
using Rhino.Mocks;

namespace CrmUp.Tests
{
    [TestFixture]
    class SolutionFileTests
    {
        [Test]
        public void Should_Load_Contents_And_Name_From_Embedded_Resource()
        {
            // arrange
            var assembly = Assembly.GetExecutingAssembly();
            var script = assembly
                .GetManifestResourceNames().OrderBy(a => a).FirstOrDefault(a => a.EndsWith(".zip"));
            if (script == null)
            {
                Assert.Fail("Embedded test solution script not found.");
            }

            // Act
            var solutionFile = CrmSolutionFile.FromEmbeddedResource(assembly, script);

            // assert
            Assert.That(solutionFile != null);
            Assert.That(solutionFile.Contents, Is.Not.Empty);
            Assert.That(solutionFile.Name, Is.EqualTo("TP888888_1_0_0_1_managed"));
        }


    }
}
