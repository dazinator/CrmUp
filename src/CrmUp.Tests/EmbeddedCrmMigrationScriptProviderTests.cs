using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DbUp.Engine;
using DbUp.Engine.Output;
using NUnit.Framework;

namespace CrmUp.Tests
{
    [Category("Script Provider")]
    public class EmbeddedCrmSolutionAndCodeMigrationProviderTests : SpecificationFor<EmbeddedCrmSolutionAndCodeMigrationProvider>
    {

        private SqlScript[] scriptsToExecute;

        public override EmbeddedCrmSolutionAndCodeMigrationProvider Given()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return new EmbeddedCrmSolutionAndCodeMigrationProvider(assembly, s => s.EndsWith(".zip"));
        }

        public override void When()
        {
            var testConnectionManager = new FakeCrmConnectionManager();
            testConnectionManager.OperationStarting(new ConsoleUpgradeLog(), new List<SqlScript>());
            scriptsToExecute = Subject.GetScripts(testConnectionManager).ToArray();
        }

        [Then]
        public void Should_Return_All_Solution_Files_And_Code_Migrations()
        {
            Assert.AreEqual(3, scriptsToExecute.Length);
        }
    }
}