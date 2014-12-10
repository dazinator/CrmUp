using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using DbUp;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Client;

namespace CrmUp.Tests.Sample_Code
{
    public static class FromManifestProgram
    {
        public static int Main(string[] args)
        {

            var manifest = new DeploymentManifest();

            // Add a step to import a solution file.            
            var solutionFileStep = new DeploymentStep();
            solutionFileStep.StepName = "Deploy Solution: TP999999_1_0_0_0_managed.zip";
            solutionFileStep.SolutionFileName = "TP999999_1_0_0_0_managed.zip";
            manifest.Steps.Add(solutionFileStep);

            var codeMigrationStep = new DeploymentStep();
            codeMigrationStep.StepName = "Code Migration: Import Something";

            // Add a step which will run our code migration class, from this assembly. See TestMigration.cs     
            // NB: To run a code migration class from another assembly, you also have to 
            // add the assembly name to the manifest.Assemblies collection.
            codeMigrationStep.CodeMigrationScriptName = "TestMigration";
            manifest.Steps.Add(codeMigrationStep);

            var upgrader =
               DeployChanges.To
                            .DynamicsCrmOrganisation("someconnectionstring")
                            .WithDeploymentManifest(manifest)
                            .LogToConsole()
                            .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            return 0;
        }
    }
}
