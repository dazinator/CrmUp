using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Discovery;

namespace CrmUp
{
    /// <summary>
    /// Classes that supply instances of Crm services will implement this interface.
    /// </summary>
    public interface ICrmServiceProvider
    {
        IOrganizationService GetOrganisationService();
        IDeploymentService GetDeploymentService();
        IDiscoveryService GetDiscoveryService();

        /// <summary>
        /// Returns an organisation service for the specified organisation using the specified credentials.
        /// </summary>
        /// <param name="org"></param>
        /// <param name="domain"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        IOrganizationService GetOrganisationService(OrganizationDetail org, string domain, string userName, string password);

        /// <summary>
        /// The connection provider used to obtain connection information for services.
        /// </summary>
        ICrmConnectionProvider ConnectionProvider { get; }

    }
}