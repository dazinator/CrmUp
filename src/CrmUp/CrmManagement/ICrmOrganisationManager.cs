using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Description;
using DbUp.Engine.Output;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Discovery;

namespace CrmUp
{
    /// <summary>
    /// An interface for performing Organization management operations in Dynamics Crm.
    /// </summary>
    public interface ICrmOrganisationManager
    {
        IEnumerable<OrganizationDetail> GetOrganisations();
        void CreateOrganization(Organization org, string sysAdminName, IUpgradeLog upgradeLog);
    }
}