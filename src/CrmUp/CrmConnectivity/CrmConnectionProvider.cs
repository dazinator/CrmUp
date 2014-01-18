using Microsoft.Xrm.Client;

namespace CrmUp
{
    public abstract class CrmConnectionProvider : ICrmConnectionProvider
    {
        public abstract CrmConnection GetOrganisationServiceConnection();
        public abstract CrmConnection GetDeploymentServiceConnection();
        public abstract CrmConnection GetDiscoveryServiceConnection();
    }
}