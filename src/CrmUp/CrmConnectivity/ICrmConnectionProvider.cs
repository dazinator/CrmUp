using Microsoft.Xrm.Client;

namespace CrmUp
{
    public interface ICrmConnectionProvider
    {
        CrmConnection GetOrganisationServiceConnection();
        CrmConnection GetDeploymentServiceConnection();
        CrmConnection GetDiscoveryServiceConnection();
    }
}