using DbUp.Engine;
using DbUp.Engine.Transactions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmUp
{
    /// <summary>
    /// An <see cref="IScriptProvider"/> implementation which retrieves solution files from a local folder.
    /// </summary>
    public class FolderDirectoryCrmSolutionScriptProvider : IScriptProvider
    {

        private readonly Func<string, bool> filter;
        private string basePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FolderDirectoryCrmSolutionScriptProvider"/> class.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <param name="filter">The filter.</param>
        public FolderDirectoryCrmSolutionScriptProvider(string basePath, Func<string, bool> filter)
        {
            // this.assembly = assembly;
            this.basePath = basePath;
            this.filter = filter;
        }

        /// <summary>
        /// Gets all scripts that should be executed.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SqlScript> GetScripts(IConnectionManager connectionManager)
        {
            var files = Directory.EnumerateFiles(basePath, "*.*", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".zip"))
            .OrderBy(x => x)
            .Select(s => CrmSolutionFile.FromFile(s))
            .ToList();

            return files;

        }

    }
}
