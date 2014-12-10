using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DbUp.Engine;
using DbUp.Engine.Output;
using NUnit.Framework;

namespace CrmUp.Tests
{
    [Category("Script Provider")]
    public class EmbeddedCrmSolutionScriptProviderTests : SpecificationFor<EmbeddedCrmSolutionScriptProvider>
    {
        private SqlScript[] scriptsToExecute;

        public override EmbeddedCrmSolutionScriptProvider Given()
        {
            var assembly = Assembly.GetExecutingAssembly();
            return new EmbeddedCrmSolutionScriptProvider(assembly, s => s.EndsWith(".zip"));
        }

        public override void When()
        {
            var testConnectionManager = new FakeCrmConnectionManager();
            testConnectionManager.OperationStarting(new ConsoleUpgradeLog(), new List<SqlScript>());
            scriptsToExecute = Subject.GetScripts(testConnectionManager).ToArray();
        }

        [Then]
        public void Should_Return_All_Solution_Files()
        {
            Assert.AreEqual(2, scriptsToExecute.Length);
        }
    }
}
 
    

