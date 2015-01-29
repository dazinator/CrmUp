using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CrmUp.Dynamics;
using CrmUp.Util;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Preprocessors;
using DbUp.Engine.Transactions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CrmUp
{

    /// <summary>
    /// An implementation of the <see cref="IScriptExecutor"/> interface which used to apply solution imports into Crm.
    /// </summary>
    public class CrmSolutionScriptExecutor : IScriptExecutor
    {
        public static object _Lock = new object();
        private Func<IConnectionManager> _ConnectionManagerFactory = null;
        private Func<IUpgradeLog> _LogFactory = null;
        private Func<bool> _VariablesEnabled = null;
        private IEnumerable<IScriptPreprocessor> _ScriptPreProcessors = null;
        private bool _EnableWriteOfJobLogToLocalFile;
        private bool _PublishWorkflows;


        public CrmSolutionScriptExecutor(Func<IConnectionManager> connectionManagerFactory, 
                                         Func<IUpgradeLog> logFactory, 
                                         Func<bool> variablesEnabled, 
                                         IEnumerable<IScriptPreprocessor> scriptPreprocessors, 
                                         bool publishWorkflows = true, bool enableWriteJobLogToFile = true)
        {
            _ConnectionManagerFactory = connectionManagerFactory;
            _LogFactory = logFactory;
            _VariablesEnabled = variablesEnabled;
            _ScriptPreProcessors = scriptPreprocessors;
            _PublishWorkflows = publishWorkflows;
            _EnableWriteOfJobLogToLocalFile = enableWriteJobLogToFile;
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
                    bool isSolution = false;
                    bool isCodeMigration = false;

                    if (script is CrmSolutionFile)
                    {
                        isSolution = true;
                    }
                    else if (script is CrmCodeMigrationScript)
                    {
                        isCodeMigration = true;
                    }

                    if (!isSolution && !isCodeMigration)
                    {
                        throw new InvalidOperationException("The migration to be applied is not of an expected type.");
                    }

                    if (isSolution)
                    {
                        ApplySolution(connectionManager, script as CrmSolutionFile);
                    }
                    else if (isCodeMigration)
                    {
                        ApplyCodeMigration(connectionManager, script as CrmCodeMigrationScript);
                    }

                    //command.CommandText = statement;
                    //if (ExecutionTimeoutSeconds != null)
                    //    command.CommandTimeout = ExecutionTimeoutSeconds.Value;
                };
            }
            catch (Exception ex)
            {
                _LogFactory().WriteInformation("Exception has occured in script: '{0}'", script.Name);
                _LogFactory().WriteError("Exception Message: {0}", ex.ToString());
                throw;
            }
        }

        private void Log(OrganizationResponse response)
        {
            _LogFactory().WriteInformation("Response from Crm name: {0} ", response.ResponseName);
            foreach (var item in response.Results)
            {
                _LogFactory().WriteInformation("Response Key / Value: {0} - {1}", item.Key, item.Value);
            }
        }

        private string WriteJobLogToFile(Guid jobId, string jobLog)
        {
            string fileName = jobId.ToString("N") + ".xls";
            string outputFileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);
            System.IO.File.WriteAllText(outputFileName, jobLog);
            return outputFileName;
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

        protected virtual void ApplySolution(IConnectionManager connectionManager, CrmSolutionFile solution)
        {
            Guid importId = Guid.NewGuid();

            var impSolReq = new ImportSolutionRequest()
            {
                CustomizationFile = solution.FileBytes,
                ImportJobId = importId,
                PublishWorkflows = _PublishWorkflows
            };

            var conn = _ConnectionManagerFactory();
            var crmConnManager = Guard.EnsureIs<CrmConnectionManager, IConnectionManager>(conn, "ConnectionManager");

            crmConnManager.ExecuteWithManagedConnection((a) =>
                {
                    var args = new MonitorProgressArgs
                     {
                         CrmServiceProvider = crmConnManager.CrmServiceProvider,
                         JobId = importId,
                         UpgradeLog = _LogFactory()
                     };
                    _LogFactory().WriteInformation("Import job id is: {0}", importId);
                    using (var timer = new Timer(new TimerCallback(ProgressReport), args, new TimeSpan(0, 0, 30), new TimeSpan(0, 1, 0)))
                    {
                        try
                        {
                            var response = a().Execute(impSolReq);
                            if (connectionManager.IsScriptOutputLogged)
                            {
                                Log(response);
                            }
                        }
                        catch (Exception e)
                        {
                            // write the job log to a file?
                            var jobLog = GetJobLog(a, importId);
                            if (_EnableWriteOfJobLogToLocalFile)
                            {                             
                               var outputFile = WriteJobLogToFile(importId, jobLog);
                               _LogFactory().WriteInformation("The import job log has been written to: {0} ", outputFile);
                            }
                           
                            throw new SolutionImportException(jobLog, e.Message, e);
                        }
                    }
                });
        }

        public class MonitorProgressArgs
        {
            public ICrmServiceProvider CrmServiceProvider { get; set; }
            public Guid JobId { get; set; }
            public IUpgradeLog UpgradeLog { get; set; }
        }

        private static void ProgressReport(object args)
        {
            // connect to crm again, don't reuse the connection that's used to import
            var monitorArgs = (MonitorProgressArgs)args;

            if (Monitor.TryEnter(_Lock, 1000))
            {
                try
                {
                    monitorArgs.UpgradeLog.WriteInformation("Checking status of the solution import..");
                    IOrganizationService orgService =
                        monitorArgs.CrmServiceProvider.GetOrganisationService();
                    using (orgService as IDisposable)
                    {
                        var job = orgService.Retrieve("importjob", (Guid)monitorArgs.JobId,
                                                      new ColumnSet("solutionname", "progress"));
                        decimal progress = Convert.ToDecimal(job["progress"]);
                        monitorArgs.UpgradeLog.WriteInformation("{0:N0}%", progress);
                        if (progress == 100)
                        {
                            return;
                        }
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    // This could occur if the main threa and disposes the objects (shared state) that are also referenced from this thread to poll for progress.
                    // Ideally this would be done in a thread safe way but for now this will suffice.
                    monitorArgs.UpgradeLog.WriteInformation(
                        "A check for the progress of the solution import was terminated..", ex.Message);
                }
                catch (Exception ex)
                {
                    monitorArgs.UpgradeLog.WriteWarning("Unable to determine the progress of the current solution import as Crm has not responded.. Will keep trying.");
                }
                finally
                {
                    Monitor.Exit(_Lock);
                }
            }
            else
            {
                monitorArgs.UpgradeLog.WriteInformation("Still awaiting solution import progress update from Crm..");
            }
        }

        protected virtual void ApplyCodeMigration(IConnectionManager connectionManager, CrmCodeMigrationScript migration)
        {
            var conn = _ConnectionManagerFactory();
            var crmConnManager = Guard.EnsureIs<CrmConnectionManager, IConnectionManager>(conn, "ConnectionManager");
            migration.Apply(crmConnManager.CrmServiceProvider, _LogFactory());
        }

        private string GetJobLog(Func<IOrganizationService> orgServiceFactory, Guid jobId)
        {

            try
            {
                var service = orgServiceFactory();
                RetrieveFormattedImportJobResultsRequest importLogRequest = new RetrieveFormattedImportJobResultsRequest()
                {
                    ImportJobId = jobId
                };
                RetrieveFormattedImportJobResultsResponse importLogResponse = (RetrieveFormattedImportJobResultsResponse)service.Execute(importLogRequest);
                return importLogResponse.FormattedResults;
            }
            catch (Exception e)
            {
                // Do nothing
                return "Unable to get job log due to an error: " + e.Message;
            }

        }

    }
}