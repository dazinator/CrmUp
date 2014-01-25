using Microsoft.Xrm.Client;

namespace CrmUp
{

    /// <summary>
    /// Single Responsibility: This is the abstract / base class for a provider that can supply CrmConnections for Crm services.
    /// </summary>
    public abstract class CrmConnectionProvider : ICrmConnectionProvider
    {
        public abstract CrmConnection GetOrganisationServiceConnection();
        public abstract CrmConnection GetDeploymentServiceConnection();
        public abstract CrmConnection GetDiscoveryServiceConnection();
    }
}