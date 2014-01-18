using CrmUp;
using DbUp.Engine.Output;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Discovery;

public class FakeCrmConnectionManager : CrmConnectionManager
{
    private ICrmServiceProvider _crmServiceProvider = null;
  
    public FakeCrmConnectionManager(ICrmServiceProvider crmServiceProvider, ICrmOrganisationManager crmOrgManager)
        : base(crmServiceProvider, crmOrgManager)
    {
        _crmServiceProvider = crmServiceProvider;
    }

    public FakeCrmConnectionManager(IOrganizationService testConnection, IDeploymentService deploymentService, IDiscoveryService discoveryService, ICrmOrganisationManager crmOrgManager)
        : base(new FakeCrmServiceProvider(testConnection, deploymentService, discoveryService), crmOrgManager)
    {
    }

    public FakeCrmConnectionManager()
        : base(new FakeCrmServiceProvider(null, null, null), null)
    {
    }


   
}