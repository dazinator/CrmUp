using Microsoft.Xrm.Client;

namespace CrmUp
{

    /// <summary>
    /// Single Responsibility: This class is responsible for providing "Crm Connnection" information for the Crm services.
    /// This implementation uses explicitly provided connection string information (properties must be set), or falls back to using the AppConfig if
    /// no connection information is explicitly provided. 
    /// </summary>
    public class ExplicitConnectionStringProviderWithFallbackToConfig : AppSettingsConnectionStringProvider
    {
        public string OrganisationServiceConnectionString { get; set; }
        public string DeploymentServiceConnectionString { get; set; }
        public string DiscoveryServiceConnectionString { get; set; }

        public override CrmConnection GetOrganisationServiceConnection()
        {
            var conn = string.IsNullOrEmpty(OrganisationServiceConnectionString)
                           ? base.GetOrganisationServiceConnection()
                           : CreateConnectionFromConnectionString(OrganisationServiceConnectionString);

            return conn;
        }

        public override CrmConnection GetDeploymentServiceConnection()
        {
            var conn = string.IsNullOrEmpty(DeploymentServiceConnectionString)
                           ? base.GetDeploymentServiceConnection()
                           : CreateConnectionFromConnectionString(DeploymentServiceConnectionString);

            return conn;
        }

        public override CrmConnection GetDiscoveryServiceConnection()
        {
            var conn = string.IsNullOrEmpty(DiscoveryServiceConnectionString)
                           ? base.GetDiscoveryServiceConnection()
                           : CreateConnectionFromConnectionString(DiscoveryServiceConnectionString);

            return conn;
        }
    }
}