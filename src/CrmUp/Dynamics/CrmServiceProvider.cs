using System;
using System.Diagnostics;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Discovery;

namespace CrmUp.Dynamics
{

    /// <summary>
    /// Single Responsibility: The responsibility of this class is to provide instances of CRM services when they are needed.
    /// Dependencies: This class depends upon an ICrmConnectionProvider to obtain Connection information for services, and 
    /// ICrmClientCredentialsProvider to obtain client credentials used service authentication.
    /// </summary>
    public class CrmServiceProvider : ICrmServiceProvider
    {

        public const string FailedToConnectErrorMessage = "Failed to connect to CRM: {0}, {1}";

        private ICrmConnectionProvider _CrmConnectionProvider;
        private ICrmClientCredentialsProvider _credentialsProvider;

        public CrmServiceProvider(ICrmConnectionProvider connectionProvider,
                                  ICrmClientCredentialsProvider credentialsProvider)
        {
            _CrmConnectionProvider = connectionProvider;
            _credentialsProvider = credentialsProvider;
        }

        public IOrganizationService GetOrganisationService()
        {
            CrmConnection connection = null;
            try
            {

                connection = _CrmConnectionProvider.GetOrganisationServiceConnection();
                connection.UserTokenExpiryWindow = new TimeSpan(0, 3, 0);
                // var service = new OrganizationService(connection);
                var context = new CrmOrganizationServiceContext(connection);
                // Optionally disbale caching?
                context.TryAccessCache(cache => cache.Mode = OrganizationServiceCacheMode.Disabled);
                return context;
            }
            catch (Exception ex)
            {
                throw new FailedToConnectToCrmException(connection, ex);
            }
        }

        public IOrganizationService GetOrganisationService(OrganizationDetail org, string domain, string userName, string password)
        {

            if (org != null)
            {
                Uri orgServiceUri = null;
                orgServiceUri = new Uri(org.Endpoints[EndpointType.OrganizationService]);
                //if (!string.IsNullOrEmpty(OrganisationServiceHostName))
                //{
                //    UriBuilder builder = new UriBuilder(orgServiceUri);
                //    builder.Host = OrganisationServiceHostName;
                //    orgServiceUri = builder.Uri;
                //}

                IServiceConfiguration<IOrganizationService> orgConfigInfo = ServiceConfigurationFactory.CreateConfiguration<IOrganizationService>(orgServiceUri);

                var creds = _credentialsProvider.GetCredentials(orgConfigInfo.AuthenticationType, domain, userName, password);
                var orgService = new OrganizationServiceProxy(orgConfigInfo, creds);
                orgService.Timeout = new TimeSpan(0, 5, 0);

                var req = new WhoAmIRequest();
                var response = (WhoAmIResponse)orgService.Execute(req);

                Debug.WriteLine(string.Format(" Connected to {0} as Crm user id: {1}", orgConfigInfo.CurrentServiceEndpoint.Address.Uri.ToString(), response.UserId));
                return orgService;
            }
            return null;

        }

        public IDeploymentService GetDeploymentService()
        {
            CrmConnection connection = null;
            try
            {
                connection = _CrmConnectionProvider.GetDeploymentServiceConnection();
                connection.UserTokenExpiryWindow = new TimeSpan(0, 3, 0);
                if (connection.Timeout == null)
                {
                    // Set a sensible timeout - 6 minutes.
                    connection.Timeout = new TimeSpan(0, 6, 0);
                }
                // var service = new OrganizationService(connection);
                var service = new DeploymentService(connection);
                return service;
            }
            catch (Exception ex)
            {
                throw new FailedToConnectToCrmException(connection, ex);
            }
        }

        public IDiscoveryService GetDiscoveryService()
        {
            CrmConnection connection = null;
            try
            {

                connection = _CrmConnectionProvider.GetDiscoveryServiceConnection();
                connection.UserTokenExpiryWindow = new TimeSpan(0, 3, 0);
                var service = new DiscoveryService(connection);
                return service;
            }
            catch (Exception ex)
            {
                throw new FailedToConnectToCrmException(connection, ex);
            }
        }

        public ICrmConnectionProvider ConnectionProvider
        {
            get { return _CrmConnectionProvider; }
        }

       

    }
}
