using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CrmUp
{
    /// <summary>
    /// An implementation of the <see cref="IJournal"/> interface which tracks version numbers for a 
    /// Dynamics Crm Organisation by querying the solutions entity in CRM.
    /// </summary>
    [Obsolete("Should use the CrmEntityJournal instead, which uses a dedicated customisable entity in Crm as opposed to solution records which are non customisable.")]
    public class CrmSolutionJournal : IJournal
    {
        // private readonly string schemaTableName;
        private readonly Func<IConnectionManager> _ConnectionManagerFactory;
        private readonly Func<IUpgradeLog> _LogFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="CrmSolutionsJournal"/> class.
        /// </summary>
        /// <param name="connectionManager">The connection manager.</param>
        /// <param name="logger">The log.</param>
        public CrmSolutionJournal(Func<IConnectionManager> connectionManager, Func<IUpgradeLog> logger)
        {
            this._ConnectionManagerFactory = connectionManager;
            _LogFactory = logger;
        }

        /// <summary>
        /// Recalls the solutions that have been applied against the crm organisation.
        /// </summary>
        /// <returns>All executed scripts.</returns>
        public string[] GetExecutedScripts()
        {
            _LogFactory().WriteInformation("Fetching list of already imported solutions.");

            var scripts = new List<string>();

            try
            {

                //  var solution = Guard.EnsureIs<CrmSolutionFile, SqlScript>(script, "Script");

                var conn = _ConnectionManagerFactory();
                var crmConnManager = Guard.EnsureIs<CrmConnectionManager, IConnectionManager>(conn, "ConnectionManager");
                crmConnManager.ExecuteWithManagedConnection((a) =>
                    {
                        var atts =
                            new ColumnSet(new string[] { "version", "uniquename", "ismanaged" });
                        //new ColumnSet(new string[] { "publisherid", "installedon", "version", "versionnumber", "friendlyname" });
                        var querySampleSolution = new QueryExpression
                            {
                                EntityName = "solution",
                                ColumnSet = atts
                            };

                        //  querySampleSolution.Criteria.AddCondition("uniquename", ConditionOperator.Equal, solutionUniqueName);
                        var results = a().RetrieveMultiple(querySampleSolution);
                        foreach (var r in results.Entities)
                        {

                            var solName = r["uniquename"];
                            var solVersion = r["version"];
                            var isManaged = (bool)r["ismanaged"];
                            var nameFormatString = "{0}_{1}{2}";
                            var managedText = string.Empty;
                            if (isManaged)
                            {
                                managedText = "_managed";
                            }
                            //  nameFormatString = string.Format(nameFormatString, solName,solVersion,

                            //var managed = r.FormattedValues["ismanaged"];

                            var name = string.Format(nameFormatString, solName, solVersion, managedText)
                                             .Replace(".", "_");
                            scripts.Add(name);
                        }
                    });

                return scripts.ToArray();
            }
            catch (Exception ex)
            {
                _LogFactory().WriteError("Exception has occured checking journal");
                _LogFactory().WriteError(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// Records a database upgrade for a database specified in a given connection string.
        /// </summary>
        /// <param name="script">The script.</param>
        public void StoreExecutedScript(SqlScript script)
        {
            // No need, as Crm keeps a record of each applied solution..
            var solution = Guard.EnsureIs<CrmSolutionFile, SqlScript>(script, "script");

        }

    }
}