using DbUp.Engine;
using DbUp.Engine.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CrmUp
{
    /// <summary>
    /// An enhanced <see cref="IScriptProvider"/> implementation which retrieves Solutions, or ICrmMigration based on a manifest.
    /// </summary>
    public class DeploymentManifestScriptProvider : IScriptProvider
    {
        // private readonly EmbeddedCrmSolutionScriptProvider embeddedScriptProvider;
        private readonly DeploymentManifest manifest;
        private Func<string, bool> filter;
        private string basePath;

        private readonly FolderDirectoryCrmSolutionScriptProvider folderSolutionFileProvider;


        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentManifestProvider"/> class.
        /// </summary>
        /// <param name="manifest">The manifest.</param>
        /// <param name="filter">The script filter.</param>
        public DeploymentManifestScriptProvider(DeploymentManifest manifest, Func<string, bool> filter)
            : this(manifest, AppDomain.CurrentDomain.BaseDirectory, filter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeploymentManifestProvider"/> class.
        /// </summary>
        /// <param name="manifest">The manifest.</param>
        /// <param name="assetBasePath">The path where solution files and code migration assemblies are located.</param>
        /// <param name="filter">The script filter.</param>
        public DeploymentManifestScriptProvider(DeploymentManifest manifest, string assetBasePath, Func<string, bool> filter)
        {
            //  this.assembly = assembly;
            this.manifest = manifest;
            this.filter = filter;
            basePath = AppDomain.CurrentDomain.BaseDirectory;
            folderSolutionFileProvider = new FolderDirectoryCrmSolutionScriptProvider(basePath, filter);
            //   embeddedScriptProvider = new EmbeddedCrmSolutionScriptProvider(assembly, filter);
        }

        private IEnumerable<SqlScript> ScriptsFromCodeMigrationClasses(IConnectionManager connectionManager)
        {
            var manifestAssemblies = from a in manifest.Assemblies select System.IO.Path.Combine(basePath, a.Name);
            var assemblies = manifestAssemblies.Select(m => System.Reflection.Assembly.LoadFile(m));


            var scriptType = typeof(CrmCodeMigration);
            var codeMigrations = assemblies.SelectMany(m => m.GetTypes().Where(type => scriptType.IsAssignableFrom(type) && type.IsClass).Select(s => new CrmCodeMigrationScript((CrmCodeMigration)Activator.CreateInstance(s))));
            //var codeMigrations = assembly
            //    .GetTypes()
            //    .Where(type => script.IsAssignableFrom(type) && type.IsClass)
            //    .Select(s => new CrmCodeMigrationScript((CrmCodeMigration)Activator.CreateInstance(s)))
            //    .ToList();
            return codeMigrations;
        }

        /// <summary>
        /// Gets all scripts that should be executed.
        /// </summary>
        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            var allScripts = folderSolutionFileProvider.GetScripts(connectionManager)
                .Concat(ScriptsFromCodeMigrationClasses(connectionManager)).ToList();

            // now check manifest.
            var manStepsWithNames = manifest.Steps.Select(a =>
            {
                return new { StepName = a.StepName, ScriptName = a.GetScriptName() };
            }).ToList();


            var manSteps = from s in manStepsWithNames
                           join a in allScripts on s.ScriptName equals a.Name into g
                           select new { step = s, script = g.DefaultIfEmpty().Single() };


            var missing = manSteps.Where(a => a.script == null).ToList();

            if (missing.Any())
            {
                throw new Exception("One or more scripts specified in the deployment manifest were not found: " + string.Join(", ", missing.Select(a => a.step.ScriptName)));
            }

            var scripts = manSteps.Select(a => a.script);
            //  .OrderBy(x => x.Name)
            return scripts;
        }
    }
}
