using System.Net;
using System.ServiceModel.Description;
using Microsoft.Xrm.Sdk.Client;

namespace CrmUp.Dynamics
{

    /// <summary>
    /// Single Responsibility: This class provides the ClientCredentials that are used for Crm web services authentication.
    /// </summary>
    public class CrmClientCredentialsProvider : ICrmClientCredentialsProvider
    {
        public ClientCredentials GetCredentials(AuthenticationProviderType providerType, string domain, string username, string password)
        {
            var creds = new ClientCredentials();
            switch (providerType)
            {
                case AuthenticationProviderType.Federation:
                case AuthenticationProviderType.LiveId:
                    if (string.IsNullOrEmpty(domain))
                    {
                        creds.UserName.UserName = username;
                        creds.UserName.Password = password;
                    }
                    else
                    {
                        creds.UserName.UserName = string.Concat(domain, @"\", username);
                        creds.UserName.Password = password;
                    }
                    break;
                case AuthenticationProviderType.ActiveDirectory:
                    creds.Windows.ClientCredential = string.IsNullOrEmpty(domain) ? new NetworkCredential(username, password) : new NetworkCredential(username, password, domain);
                    break;
                default:
                    break;
            }
            return creds;
        }

    }
}