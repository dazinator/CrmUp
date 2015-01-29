using System;
using System.Reflection;
using CrmUp.Dynamics;
using CrmUp.Fluent;
using CrmUp.Util;
using DbUp.Builder;
using DbUp.Engine;
using DbUp.Engine.Transactions;
using DbUp.Support.SqlServer;
using Microsoft.Xrm.Sdk.Deployment;

// ReSharper disable CheckNamespace 
// Do not change the namespace as we want anyone impprting the dynamics Sdk to get these handy utility extension methods without having
// to set up additional Using / Imports statements.
namespace CrmUp
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// Exposes fluent API methods for DynamicsUpgradeEngineBuilder.
    /// </summary>
    public static class CrmUpExtensions
    {

        #region To DynamicsCrmOrganisation

        public static DynamicsUpgradeEngineBuilder DynamicsCrmOrganisation(this SupportedDatabases supported, string organisationConnectionString = "", bool publishWorkflows = true, bool enableWriteJobLogsToFile = true)
        {
            var crmConnectionProvider = new ExplicitConnectionStringProviderWithFallbackToConfig();
            crmConnectionProvider.OrganisationServiceConnectionString = organisationConnectionString;
            var credentialsProvider = new CrmClientCredentialsProvider();
            var crmServiceProvider = new CrmServiceProvider(crmConnectionProvider, credentialsProvider);
            return DynamicsCrmOrganisation(supported, crmServiceProvider, publishWorkflows, enableWriteJobLogsToFile);
        }

        public static DynamicsUpgradeEngineBuilder DynamicsCrmOrganisation(this SupportedDatabases supported, ICrmClientCredentialsProvider clientCredentialsProvider)
        {
            var crmConnectionProvider = new ExplicitConnectionStringProviderWithFallbackToConfig();
            return DynamicsCrmOrganisation(supported, crmConnectionProvider, clientCredentialsProvider);
        }

        public static DynamicsUpgradeEngineBuilder DynamicsCrmOrganisation(this SupportedDatabases supported, ICrmConnectionProvider connectionProvider)
        {
            var credentialsProvider = new CrmClientCredentialsProvider();
            return DynamicsCrmOrganisation(supported, connectionProvider, credentialsProvider);
        }

        public static DynamicsUpgradeEngineBuilder DynamicsCrmOrganisation(this SupportedDatabases supported, ICrmConnectionProvider connectionProvider, ICrmClientCredentialsProvider clientCredentialsProvider)
        {
            var crmServiceProvider = new CrmServiceProvider(connectionProvider, clientCredentialsProvider);
            return DynamicsCrmOrganisation(supported, crmServiceProvider);
        }
              
        public static DynamicsUpgradeEngineBuilder DynamicsCrmOrganisation(this SupportedDatabases supported, ICrmServiceProvider crmServiceProvider, bool publishWorkflows = true, bool enableWriteJobLogsToFile = true)
        {
            return DynamicsCrmUpgradeEngineBuilder(new CrmConnectionManager(crmServiceProvider), new CrmOrganisationManager(crmServiceProvider), publishWorkflows, enableWriteJobLogsToFile);
        }

        #endregion

        #region With Migrations


        /// <summary>
        /// Adds all solution files found and code based migrations as embedded resources in the given assembly.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        /// The same builder
        /// </returns>
        public static DynamicsUpgradeEngineBuilder WithSolutionsAndMigrationsEmbeddedInAssembly(this DynamicsUpgradeEngineBuilder builder, Assembly assembly)
        {
            return WithSolutionsAndMigrationsEmbeddedInAssembly(builder, assembly, s => s.EndsWith(".zip", StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Adds all solution files found as embedded resources in the given assembly, as well as code based migrations.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assembly">The assembly.</param>
        /// <param name="filter"></param>
        /// <returns>
        /// The same builder
        /// </returns>
        public static DynamicsUpgradeEngineBuilder WithSolutionsAndMigrationsEmbeddedInAssembly(this DynamicsUpgradeEngineBuilder builder, Assembly assembly, Func<string, bool> filter)
        {
            return builder.WithScripts(new EmbeddedCrmSolutionAndCodeMigrationProvider(assembly, filter)) as DynamicsUpgradeEngineBuilder;
        }


        /// <summary>
        /// Adds all solution files found as embedded resources in the given assembly.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="assembly">The assembly.</param>
        /// <returns>
        /// The same builder
        /// </returns>
        public static DynamicsUpgradeEngineBuilder WithSolutionsEmbeddedInAssembly(this DynamicsUpgradeEngineBuilder builder, Assembly assembly)
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
        public static DynamicsUpgradeEngineBuilder WithSolutionsEmbeddedInAssembly(this DynamicsUpgradeEngineBuilder builder, Assembly assembly, Func<string, bool> filter)
        {
            return builder.WithScripts(new EmbeddedCrmSolutionScriptProvider(assembly, filter)) as DynamicsUpgradeEngineBuilder;
        }

        /// <summary>
        /// Includes the solutions, and code migrations specified in the deployment manifest.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="manifest">The deployment manifest.</param>
        /// <returns>
        /// The same builder
        /// </returns>
        public static DynamicsUpgradeEngineBuilder WithDeploymentManifest(this DynamicsUpgradeEngineBuilder builder, DeploymentManifest manifest)
        {
            return WithDeploymentManifest(builder, manifest, null);
        }

        /// <summary>
        /// Includes the solutions, and code migrations specified in the deployment manifest.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="manifest">The deployment manifest.</param>
        /// <param name="filter"></param>
        /// <returns>
        /// The same builder
        /// </returns>
        public static DynamicsUpgradeEngineBuilder WithDeploymentManifest(this DynamicsUpgradeEngineBuilder builder, DeploymentManifest manifest, Func<string, bool> filter)
        {
            return builder.WithScripts(new DeploymentManifestScriptProvider(manifest, filter)) as DynamicsUpgradeEngineBuilder;
        }



        #endregion

        #region Create Organisation If Doesn't Exist

        public static DynamicsUpgradeEngineBuilder CreateIfDoesNotExist(this DynamicsUpgradeEngineBuilder builder, Func<CreateOrganisationArgs> createOrgArgs, string discoveryServiceConnectionString = "", string deploymentServiceConnectionString = "")
        {
            builder.Configure(a =>
            {
                // Ensure the connection manager is set up with required connection strings for the discovery and deployment services.
                var conn = Guard.EnsureIs<CrmConnectionManager, IConnectionManager>(a.ConnectionManager, "connectionManager");
                // Need to configure the deployment service url..
                var connProvider = conn.CrmServiceProvider.ConnectionProvider as ExplicitConnectionStringProviderWithFallbackToConfig;
                if (connProvider != null)
                {
                    connProvider.DiscoveryServiceConnectionString = discoveryServiceConnectionString;
                    connProvider.DeploymentServiceConnectionString = deploymentServiceConnectionString;
                }
                // ensure the journal is set to create org, with callback to reqtrieve organisation information for create.
                var journal = Guard.EnsureIs<ISupportOrgCreation, IJournal>(a.Journal, "journal");
                journal.EnsureOrganisationExists = createOrgArgs;
            });
            return builder;
        }

        #endregion

        private static DynamicsUpgradeEngineBuilder DynamicsCrmUpgradeEngineBuilder(IConnectionManager connectionManager, CrmOrganisationManager crmOrgManager, bool publishWorkflows = true, bool writeJobLogToFile = true)
        {
            var builder = new DynamicsUpgradeEngineBuilder();
            builder.Configure(c => c.ConnectionManager = connectionManager);
            builder.Configure(c => c.ScriptExecutor = new CrmSolutionScriptExecutor(() => c.ConnectionManager, () => c.Log, () => c.VariablesEnabled, c.ScriptPreprocessors, publishWorkflows, writeJobLogToFile));
            builder.Configure(c => c.Journal = new CrmEntityJournal(() => c.ConnectionManager, () => c.Log, crmOrgManager));
            return builder;
        }


    }
}