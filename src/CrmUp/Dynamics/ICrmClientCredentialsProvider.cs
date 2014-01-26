using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk.Client;

namespace CrmUp.Dynamics
{
    /// <summary>
    /// Classes that supply ClientCredentials for Crm webservice authentication will implement this interface. 
    /// </summary>
    public interface ICrmClientCredentialsProvider
    {
        ClientCredentials GetCredentials(AuthenticationProviderType providerType, string domain, string username,
                                         string password);
    }
}