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
    public class DeploymentManifestScriptProviderTests
    {

        [Test]
        public void Should_Return_Only_Solution_Files_In_Manifest()
        {

            var manifest = new DeploymentManifest();
            var deploymentStep = new DeploymentStep();
            deploymentStep.StepName = "Deploy Solution: TP888888_1_0_0_1_managed.zip";
            deploymentStep.SolutionFileName = "TP888888_1_0_0_1_managed.zip";
            manifest.Steps.Add(deploymentStep);

            var currentDir = System.IO.Directory.GetCurrentDirectory();

            var sut = new DeploymentManifestScriptProvider(manifest, currentDir, null);

            var testConnectionManager = new FakeCrmConnectionManager();
            testConnectionManager.OperationStarting(new ConsoleUpgradeLog(), new List<SqlScript>());
            var scriptsToExecute = sut.GetScripts(testConnectionManager).ToArray();
            Assert.AreEqual(1, scriptsToExecute.Length);
        }

        [ExpectedException(MatchType = MessageMatch.Contains, ExpectedMessage = "IDontExist")]
        [Test]
        public void Should_Throw_When_Solution_File_Specified_In_Manifest_Not_Found()
        {

            var manifest = new DeploymentManifest();
            var deploymentStep = new DeploymentStep();
            deploymentStep.StepName = "Deploy Solution: TP888888_1_0_0_1_managed.zip";
            deploymentStep.SolutionFileName = "TP888888_1_0_0_1_managed.zip";
            manifest.Steps.Add(deploymentStep);
            var deploymentStep2 = new DeploymentStep();
            deploymentStep2.StepName = "Deploy Solution: IDontExist.zip";
            deploymentStep2.SolutionFileName = "IDontExist.zip";
            manifest.Steps.Add(deploymentStep2);

            var currentDir = System.IO.Directory.GetCurrentDirectory();
            var sut = new DeploymentManifestScriptProvider(manifest, currentDir, null);

            var testConnectionManager = new FakeCrmConnectionManager();
            testConnectionManager.OperationStarting(new ConsoleUpgradeLog(), new List<SqlScript>());
            var scriptsToExecute = sut.GetScripts(testConnectionManager).ToArray();
        }

        [Test]
        public void Should_Return_Code_Migrations_Specified_In_Manifest()
        {

            var manifest = new DeploymentManifest();

            var deploymentAssembly = new DeploymentAssembly();
            deploymentAssembly.Name = "CrmUp.Tests.dll";
            manifest.Assemblies.Add(deploymentAssembly);

            var deploymentStep = new DeploymentStep();
            deploymentStep.StepName = "Code Migration: Import Something";
            deploymentStep.CodeMigrationScriptName = "TestMigration";
            manifest.Steps.Add(deploymentStep);

            var currentDir = System.IO.Directory.GetCurrentDirectory();
            var sut = new DeploymentManifestScriptProvider(manifest, currentDir, null);

            var testConnectionManager = new FakeCrmConnectionManager();
            testConnectionManager.OperationStarting(new ConsoleUpgradeLog(), new List<SqlScript>());
            var scriptsToExecute = sut.GetScripts(testConnectionManager).ToArray();

            Assert.AreEqual(1, scriptsToExecute.Length);
        }

        [ExpectedException(MatchType = MessageMatch.Contains, ExpectedMessage = "IDontExist")]
        [Test]
        public void Should_Throw_When_Code_Migration_Specified_In_Manifest_Not_Found()
        {

            var manifest = new DeploymentManifest();

            var deploymentAssembly = new DeploymentAssembly();
            deploymentAssembly.Name = "CrmUp.Tests.dll";
            manifest.Assemblies.Add(deploymentAssembly);

            var deploymentStep = new DeploymentStep();
            deploymentStep.StepName = "Code Migration: Import Something";
            deploymentStep.CodeMigrationScriptName = "IDontExist";
            manifest.Steps.Add(deploymentStep);

            var currentDir = System.IO.Directory.GetCurrentDirectory();
            var sut = new DeploymentManifestScriptProvider(manifest, currentDir, null);

            var testConnectionManager = new FakeCrmConnectionManager();
            testConnectionManager.OperationStarting(new ConsoleUpgradeLog(), new List<SqlScript>());
            var scriptsToExecute = sut.GetScripts(testConnectionManager).ToArray();
        }

        [Test]
        public void Should_Return_Solution_Files_And_Code_Migrations_In_Manifest()
        {

            var manifest = new DeploymentManifest();

            // Add a solution file step.
            var deploymentStep = new DeploymentStep();
            deploymentStep.StepName = "Deploy Solution: TP888888_1_0_0_1_managed.zip";
            deploymentStep.SolutionFileName = "TP888888_1_0_0_1_managed.zip";
            manifest.Steps.Add(deploymentStep);

            // Add an assembly that contains code migrations to use on the deployment.
            var deploymentAssembly = new DeploymentAssembly();
            deploymentAssembly.Name = "CrmUp.Tests.dll";
            manifest.Assemblies.Add(deploymentAssembly);

            // Add a code migration step.
            var deploymentStep2 = new DeploymentStep();
            deploymentStep2.StepName = "Code Migration: Import Something";
            deploymentStep2.CodeMigrationScriptName = "TestMigration";
            manifest.Steps.Add(deploymentStep2);

            // Add another solution file step.
            var deploymentStep3 = new DeploymentStep();
            deploymentStep3.StepName = "Deploy Solution: TP999999_1_0_0_0_managed.zip";
            deploymentStep3.SolutionFileName = "TP999999_1_0_0_0_managed.zip";
            manifest.Steps.Add(deploymentStep3);

            var currentDir = System.IO.Directory.GetCurrentDirectory();
            var sut = new DeploymentManifestScriptProvider(manifest, currentDir, null);

            var testConnectionManager = new FakeCrmConnectionManager();
            testConnectionManager.OperationStarting(new ConsoleUpgradeLog(), new List<SqlScript>());

            var scriptsToExecute = sut.GetScripts(testConnectionManager).ToArray();
            Assert.AreEqual(3, scriptsToExecute.Length);

            // They should be in the order declared in the manifest file.
            var first = scriptsToExecute[0];
            Assert.AreEqual(first.Name, deploymentStep.GetScriptName());

            var second = scriptsToExecute[1];
            Assert.AreEqual(second.Name, deploymentStep2.GetScriptName());

            var third = scriptsToExecute[2];
            Assert.AreEqual(third.Name, deploymentStep3.GetScriptName());

            manifest.Save("C://DeploymentManifest.xml");

        }

    }

}



