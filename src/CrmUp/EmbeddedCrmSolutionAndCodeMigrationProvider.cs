using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DbUp.Engine;
using DbUp.Engine.Transactions;

namespace CrmUp
{
    /// <summary>
    /// An enhanced <see cref="IScriptProvider"/> implementation which retrieves Solutions, or ICrmMigration based migrations embedded in an assembly.
    /// </summary>
    public class EmbeddedCrmSolutionAndCodeMigrationProvider : IScriptProvider
    {
        private readonly EmbeddedCrmSolutionScriptProvider embeddedScriptProvider;
        private readonly Assembly assembly;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedCrmSolutionAndCodeProvider"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="filter">The embedded sql script filter.</param>
        public EmbeddedCrmSolutionAndCodeMigrationProvider(Assembly assembly, Func<string, bool> filter)
        {
            this.assembly = assembly;
            embeddedScriptProvider = new EmbeddedCrmSolutionScriptProvider(assembly, filter);
        }

        private IEnumerable<SqlScript> ScriptsFromCodeMigrationClasses(IConnectionManager connectionManager)
        {
            var script = typeof (CrmCodeMigration);
            var codeMigrations = assembly
                .GetTypes()
                .Where(type => script.IsAssignableFrom(type) && type.IsClass)
                .Select(s => new CrmCodeMigrationScript((CrmCodeMigration) Activator.CreateInstance(s)))
                .ToList();
            return codeMigrations;
        }

        /// <summary>
        /// Gets all scripts that should be executed.
        /// </summary>
        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            var sqlScripts = embeddedScriptProvider
                .GetScripts(connectionManager)
                .Concat(ScriptsFromCodeMigrationClasses(connectionManager))
                .OrderBy(x => x.Name)
                .ToList();
            return sqlScripts;
        }
    }
}