using System;
using System.Configuration;
using Microsoft.Xrm.Client;

namespace CrmUp.Dynamics
{
    /// <summary>
    /// Single Responsibility: This class provides "CrmConnnections" and uses the AppConfig file to provide such connections. 
    /// </summary>
    public abstract class AppSettingsConnectionStringProvider : CrmConnectionProvider
    {
        public const string DeploymentConnectionStringKey = "CrmDeploymentServiceConnectionString";
        public const string DiscoveryConnectionStringKey = "CrmDiscoveryServiceConnectionString";
        public const string OrgConnectionStringKey = "CrmOrganisationServiceConnectionString";

        public override CrmConnection GetOrganisationServiceConnection()
        {
            return CreateConnectionFromConnectionStringInConfigFile(OrgConnectionStringKey);
        }

        public override CrmConnection GetDeploymentServiceConnection()
        {
            return CreateConnectionFromConnectionStringInConfigFile(DeploymentConnectionStringKey);
        }

        public override CrmConnection GetDiscoveryServiceConnection()
        {
            return CreateConnectionFromConnectionStringInConfigFile(DiscoveryConnectionStringKey);
        }

        protected CrmConnection CreateConnectionFromConnectionStringInConfigFile(string key)
        {
            var connStringSetting = ConfigurationManager.ConnectionStrings[key];
            if (connStringSetting == null)
            {
                throw new ArgumentException("Connection string for a required Crm service was not found in the connectionStrings section of your config file. The missing connection string name is:" + key);
            }
            return CreateConnectionFromConnectionString(connStringSetting.ConnectionString);
        }

        protected CrmConnection CreateConnectionFromConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.");

            }
            return CrmConnection.Parse(connectionString);
        }
    }
}