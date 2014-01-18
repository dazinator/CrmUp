using Microsoft.Xrm.Client;

namespace CrmUp
{
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