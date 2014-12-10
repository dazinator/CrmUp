using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DbUp.Engine;
using DbUp.Engine.Transactions;
using DbUp.ScriptProviders;
using System.IO;

namespace CrmUp
{
    /// <summary>
    /// An <see cref="IScriptProvider"/> implementation which retrieves solution files embedded in an assembly.
    /// </summary>
    public class EmbeddedCrmSolutionScriptProvider : IScriptProvider
    {
        private readonly Assembly assembly;
        private readonly Func<string, bool> filter;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmbeddedScriptProvider"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="filter">The filter.</param>
        public EmbeddedCrmSolutionScriptProvider(Assembly assembly, Func<string, bool> filter)
        {
            this.assembly = assembly;
            this.filter = filter;
        }

        /// <summary>
        /// Gets all scripts that should be executed.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            //NOTE: I am removing the namespace and folder information present in the ManifestResourceName from the script name, so only the filename is used.
            // This is because the CRM journal table (solution entity) can not store this additional information in it, so it 
            // can never be used when querying CRM to determine if a solution has allready been imported - making it pointless.
            return assembly
                .GetManifestResourceNames()
                .Where(filter)
                .OrderBy(x => x)
                .Select(s => CrmSolutionFile.FromEmbeddedResource(assembly, s))
                .ToList();
        }

    }

 
}