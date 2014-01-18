using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk.Client;

namespace CrmUp
{
    public interface ICrmClientCredentialsProvider
    {
        ClientCredentials GetCredentials(AuthenticationProviderType providerType, string domain, string username,
                                         string password);
    }
}