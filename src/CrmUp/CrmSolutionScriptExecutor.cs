using System;
using System.Collections.Generic;
using System.Linq;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Preprocessors;
using DbUp.Engine.Transactions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;

namespace CrmUp
{

    /// <summary>
    /// An implementation of the <see cref="IScriptExecutor"/> interface which used to apply solution imports into Crm.
    /// </summary>
    public class CrmSolutionScriptExecutor : IScriptExecutor
    {

        private Func<IConnectionManager> _ConnectionManagerFactory = null;
        private Func<IUpgradeLog> _LogFactory = null;
        private Func<bool> _VariablesEnabled = null;
        private IEnumerable<IScriptPreprocessor> _ScriptPreProcessors = null;
      
        public CrmSolutionScriptExecutor(Func<IConnectionManager> connectionManagerFactory, Func<IUpgradeLog> logFactory, Func<bool> variablesEnabled, IEnumerable<IScriptPreprocessor> scriptPreprocessors)
        {
            _ConnectionManagerFactory = connectionManagerFactory;
            _LogFactory = logFactory;
            _VariablesEnabled = variablesEnabled;
            _ScriptPreProcessors = scriptPreprocessors;
        }

        /// <summary>
        /// Executes the specified script against the target system.
        /// </summary>
        /// <param name="script">The script to apply, this should be an instance of <see cref="IScriptExecutor"/></param>
        public void Execute(SqlScript script)
        {
            Execute(script, null);
        }

        /// <summary>
        /// Executes the specified script against the target system.
        /// </summary>
        /// <param name="script">The script to apply, this should be an instance of <see cref="IScriptExecutor"/></param>
        /// <param name="variables">Variables to replace in the script</param>
        public void Execute(SqlScript script, IDictionary<string, string> variables)
        {

            if (variables == null)
                variables = new Dictionary<string, string>();

            _LogFactory().WriteInformation("Executing Crm Solution Import '{0}'", script.Name);

            var contents = script.Contents;
            //if (string.IsNullOrEmpty(Schema))
            //    contents = new StripSchemaPreprocessor().Process(contents);
            if (_VariablesEnabled())
                contents = new VariableSubstitutionPreprocessor(variables).Process(contents);
            contents = (_ScriptPreProcessors ?? new IScriptPreprocessor[0])
                .Aggregate(contents, (current, additionalScriptPreprocessor) => additionalScriptPreprocessor.Process(current));

            var connectionManager = _ConnectionManagerFactory();

            var scriptStatements = connectionManager.SplitScriptIntoCommands(contents);
            var index = -1;
            try
            {

                foreach (var statement in scriptStatements)
                {
                    index++;

                    var solution = Guard.EnsureIs<CrmSolutionFile, SqlScript>(script, "Script");
                    var impSolReq = new ImportSolutionRequest()
                    {
                        CustomizationFile = solution.FileBytes
                    };

                    var conn = _ConnectionManagerFactory();
                    var crmConnManager = Guard.EnsureIs<CrmConnectionManager, IConnectionManager>(conn, "ConnectionManager");
                    crmConnManager.ExecuteWithManagedConnection((a) =>
                    {
                        var response = a().Execute(impSolReq);
                        if (connectionManager.IsScriptOutputLogged)
                        {
                            Log(response);
                        }
                    });
                    //command.CommandText = statement;
                    //if (ExecutionTimeoutSeconds != null)
                    //    command.CommandTimeout = ExecutionTimeoutSeconds.Value;
                };
            }
            catch (Exception ex)
            {
                _LogFactory().WriteInformation("Exception has occured in script: '{0}'", script.Name);
                _LogFactory().WriteError(ex.ToString());
                throw;
            }
        }

        private void Log(OrganizationResponse response)
        {
            _LogFactory().WriteInformation("Response from Crm: ");
            _LogFactory().WriteInformation(response.ResponseName);
            _LogFactory().WriteInformation("Response Params: ");
            foreach (var item in response.Results)
            {
                _LogFactory().WriteInformation("{0} - {1}", item.Key, item.Value);

            }
        }

        /// <summary>
        /// Having to implement this due to DbUp interfac, but nothing to do.
        /// </summary>
        public void VerifySchema()
        {
            // nothing to do here.. There is no schema as such for a solution file.
        }

        /// <summary>
        /// Having to implement this due to DbUp interface, but nothing to do.
        /// </summary>
        public int? ExecutionTimeoutSeconds { get; set; }

    
    }
}