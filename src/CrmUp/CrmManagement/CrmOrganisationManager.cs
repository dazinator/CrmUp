using System.Collections.Generic;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Discovery;

namespace CrmUp
{
    public class CrmOrganisationManager : ICrmOrganisationManager
    {
        private ICrmServiceProvider _crmServiceProvider;

        public CrmOrganisationManager(ICrmServiceProvider crmConnectionProvider)
        {
            _crmServiceProvider = crmConnectionProvider;
        }

        public IEnumerable<OrganizationDetail> GetOrganisations()
        {
            var dsp = _crmServiceProvider.GetDiscoveryService();
            var orgRequest = new RetrieveOrganizationsRequest();
            var orgResponse = dsp.Execute(orgRequest) as RetrieveOrganizationsResponse;
            if (orgResponse != null)
            {
                return orgResponse.Details;
            }
            return new List<OrganizationDetail>();
        }

        public void CreateOrganization(Organization orgToCheck)
        {
            //TODO: Create Org..
            throw new System.NotImplementedException();
        }
    }
}