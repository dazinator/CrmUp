using CrmUp.Tests;
using DbUp.Builder;
using DbUp.Engine.Transactions;
using DbUp.Support.SqlServer;

namespace CrmUp
{
    public static class CrmUpExtensions
    {
        public static UpgradeEngineBuilder DynamicsCrm(this SupportedDatabases supported, string connectionStringOrKey, bool isConnectionStringKey)
        {
            return CrmOrganisation(new CrmConnectionManager(connectionStringOrKey, isConnectionStringKey));
        }

        private static UpgradeEngineBuilder CrmOrganisation(IConnectionManager connectionManager)
        {
            var builder = new UpgradeEngineBuilder();
            builder.Configure(c => c.ConnectionManager = connectionManager);
            builder.Configure(c => c.ScriptExecutor = new CrmSolutionScriptExecutor(() => c.ConnectionManager, () => c.Log, () => c.VariablesEnabled, c.ScriptPreprocessors));
            builder.Configure(c => c.Journal = new CrmSolutionJournal(() => c.ConnectionManager, () => c.Log));
            return builder;
        }
    }
}