using System;
using System.Reflection;
using DbUp.Builder;
using DbUp.Engine.Transactions;
using DbUp.Support.SqlServer;
using Microsoft.Xrm.Sdk.Deployment;

namespace CrmUp
{
    public static class CrmUpExtensions
    {

        public static UpgradeEngineBuilder DynamicsCrmOrganisation(this SupportedDatabases supported, string organisationConnectionString = "")
        {
            var crmConnectionProvider = new ExplicitConnectionStringProviderWithFallbackToConfig();
            crmConnectionProvider.OrganisationServiceConnectionString = organisationConnectionString;
            var credentialsProvider = new CrmClientCredentialsProvider();
            var crmServiceProvider = new CrmServiceProvider(crmConnectionProvider, credentialsProvider);
            var crmOrgManager = new CrmOrganisationManager(crmServiceProvider);
            return DynamicsCrmUpgradeEngineBuilder(new CrmConnectionManager(crmServiceProvider, crmOrgManager));
        }

        public static UpgradeEngineBuilder CreateIfDoesNotExist(this UpgradeEngineBuilder builder, Func<Organization> orgToCreate, string discoveryServiceConnectionString = "")
        {
            builder.Configure(a =>
            {
                var conn = Guard.EnsureIs<CrmConnectionManager, IConnectionManager>(a.ConnectionManager, "connectionManager");
                // Need to configure the deployment service url..
                var connProvider = conn.CrmServiceProvider.ConnectionProvider as ExplicitConnectionStringProviderWithFallbackToConfig;
                if (connProvider != null)
                    connProvider.DiscoveryServiceConnectionString = discoveryServiceConnectionString;
            });
            return builder;
        }

        #region DI Overloads
        public static UpgradeEngineBuilder DynamicsCrmOrganisation(this SupportedDatabases supported, ICrmClientCredentialsProvider clientCredentialsProvider)
        {
            var crmConnectionProvider = new ExplicitConnectionStringProviderWithFallbackToConfig();
            return DynamicsCrmOrganisation(supported, crmConnectionProvider, clientCredentialsProvider);
        }

        public static UpgradeEngineBuilder DynamicsCrmOrganisation(this SupportedDatabases supported, ICrmConnectionProvider connectionProvider)
        {
            var credentialsProvider = new CrmClientCredentialsProvider();
            return DynamicsCrmOrganisation(supported, connectionProvider, credentialsProvider);
        }

        public static UpgradeEngineBuilder DynamicsCrmOrganisation(this SupportedDatabases supported, ICrmConnectionProvider connectionProvider, ICrmClientCredentialsProvider clientCredentialsProvider)
        {
            var crmServiceProvider = new CrmServiceProvider(connectionProvider, clientCredentialsProvider);
            return DynamicsCrmOrganisation(supported, crmServiceProvider);
        }

        public static UpgradeEngineBuilder DynamicsCrmOrganisation(this SupportedDatabases supported, ICrmServiceProvider crmServiceProvider)
        {
            var crmOrgManager = new CrmOrganisationManager(crmServiceProvider);
            return DynamicsCrmUpgradeEngineBuilder(new CrmConnectionManager(crmServiceProvider, crmOrgManager));
        }

        private static UpgradeEngineBuilder DynamicsCrmUpgradeEngineBuilder(IConnectionManager connectionManager)
        {
            var builder = new UpgradeEngineBuilder();
            builder.Configure(c => c.ConnectionManager = connectionManager);
            builder.Configure(c => c.ScriptExecutor = new CrmSolutionScriptExecutor(() => c.ConnectionManager, () => c.Log, () => c.VariablesEnabled, c.ScriptPreprocessors));
            builder.Configure(c => c.Journal = new CrmSolutionJournal(() => c.ConnectionManager, () => c.Log));
            return builder;
        }
        #endregion

        /// <summary>
        /// Adds all solution files found as embedded resources in the given assembly.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        /// The same builder
        /// </returns>
        public static UpgradeEngineBuilder WithSolutionsEmbeddedInAssembly(this UpgradeEngineBuilder builder, Assembly assembly)
        {
            return WithSolutionsEmbeddedInAssembly(builder, assembly, s => s.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Adds all solution files found as embedded resources in the given assembly.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="filter"></param>
        /// <returns>
        /// The same builder
        /// </returns>
        public static UpgradeEngineBuilder WithSolutionsEmbeddedInAssembly(this UpgradeEngineBuilder builder, Assembly assembly, Func<string, bool> filter)
        {
            return builder.WithScripts(new EmbeddedCrmSolutionScriptProvider(assembly, filter));
        }
    }
}