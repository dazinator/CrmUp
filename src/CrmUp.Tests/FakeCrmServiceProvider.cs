using System;
using CrmUp;
using CrmUp.Dynamics;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Discovery;

public class FakeCrmServiceProvider : ICrmServiceProvider
{
    private readonly IOrganizationService _testConnection;
    private readonly IDeploymentService _deploymentService;
    private readonly IDiscoveryService _discoveryService;



    public FakeCrmServiceProvider(IOrganizationService testConnection,
                                  IDeploymentService deploymentService,
                                  IDiscoveryService discoveryService
         )
    {
        _testConnection = testConnection;
        _deploymentService = deploymentService;
        _discoveryService = discoveryService;

    }

    public IOrganizationService GetOrganisationService()
    {
        return _testConnection;
    }

    public IDeploymentService GetDeploymentService()
    {
        return _deploymentService;
    }

    public IDiscoveryService GetDiscoveryService()
    {
        return _discoveryService;
    }

    public IOrganizationService GetOrganisationService(OrganizationDetail org, string domain, string userName, string password)
    {
        throw new NotImplementedException();
    }

    public ICrmConnectionProvider ConnectionProvider
    {
        get { return new ExplicitConnectionStringProviderWithFallbackToConfig(); }
    }
}