using System;
using System.Configuration;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Discovery;

namespace CrmUp
{
    public class OrganizationProxyFactory : IOrganizationProxyFactory
    {

        public const string DiscoSuffix = @"/XRMServices/2011/Discovery.svc";
        public const string FailedToConnectErrorMessage = "Failed to connect to CRM: {0}, {1}";

        private string _connectionString = string.Empty;

        public OrganizationProxyFactory(string connectionStringOrKey, bool isConfigKey = false)
        {
            ConnectionStringSettings connString = null;
            if (isConfigKey)
            {
                connString = ConfigurationManager.ConnectionStrings[connectionStringOrKey];
                _connectionString = connString.ConnectionString;
            }
            else
            {
                _connectionString = connectionStringOrKey;
            }
        }

        #region Properties

        private OrganizationDetailCollection Orgs;

        #endregion

        #region Helper Methods


        //private ClientCredentials ObtainCredentials(AuthenticationProviderType providerType)
        //{
        //    var creds = new ClientCredentials();

        //    switch (providerType)
        //    {
        //        case AuthenticationProviderType.Federation:
        //        case AuthenticationProviderType.LiveId:
        //            if (string.IsNullOrEmpty(Domain))
        //            {
        //                creds.UserName.UserName = UserName;
        //                creds.UserName.Password = Password;
        //            }
        //            else
        //            {
        //                creds.UserName.UserName = string.Concat(Domain, @"\", UserName);
        //                creds.UserName.Password = Password;
        //            }
        //            break;
        //        case AuthenticationProviderType.ActiveDirectory:
        //            creds.Windows.ClientCredential = string.IsNullOrEmpty(Domain) ?
        //                                                 new NetworkCredential(UserName, Password) : new NetworkCredential(UserName, Password, Domain);
        //            break;
        //        default:
        //            break;
        //    }

        //    return creds;
        //}

        #endregion

        /// <summary>
        /// Function that connects to the CRM Services.
        /// </summary>
        /// <returns></returns>
        public IOrganizationService CreateOrganizationServiceProxy()
        {
            #region Logging
            //Debug.WriteLine(string.Format("Trying to connect to CRM {0} with credentials:", string.Concat(CrmUrl, @"/", OrganisationalName)));
            //  Debug.WriteLine(string.Format(" Domain = {0}; Username = {1}; Password = {2}", Domain, UserName, Password));
            #endregion
            // bool connected = false;
            string crmUrl = String.Empty;
            try
            {
                crmUrl = _connectionString;
                var connection = CrmConnection.Parse(crmUrl);
                connection.UserTokenExpiryWindow = new TimeSpan(0, 3, 0);
                // var service = new OrganizationService(connection);
                var context = new CrmOrganizationServiceContext(connection);
                // Optionally disbale caching?
                context.TryAccessCache(cache => cache.Mode = OrganizationServiceCacheMode.Disabled);
                return context;
            }
            catch (Exception ex)
            {
                throw new FailedToConnectToCrmException(ex);
            }

        }


    }
}