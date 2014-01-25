using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Threading;
using System.Xml;
using DbUp.Engine.Output;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Discovery;
using OrganizationState = Microsoft.Xrm.Sdk.Deployment.OrganizationState;

namespace CrmUp
{
    /// <summary>
    /// Single Responsibility: To provide management functions for Crm organisations, such as Creation, and Retreival.
    /// </summary>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="org"></param>
        /// <param name="sysAdminName">domain\\user</param>
        public void CreateOrganization(Organization org, string sysAdminName, IUpgradeLog upgradeLog)
        {
            try
            {
                var service = _crmServiceProvider.GetDeploymentService();
                using (service as IDisposable)
                {
                    //
                    var request = new BeginCreateOrganizationRequest
                    {
                        Organization = org,
                        SysAdminName = sysAdminName
                    };

                    // Execute the request
                    upgradeLog.WriteInformation("Creating new Crm Organisation: {0}", org.UniqueName);
                    var response = (BeginCreateOrganizationResponse)service.Execute(request);

                    // The operation is asynchronous, so the response object contains
                    // a unique identifier for the operation
                    Guid operationId = response.OperationId;

                    // Retrieve the Operation using the OperationId
                    var retrieveOperationStatus = new RetrieveRequest();
                    retrieveOperationStatus.EntityType = DeploymentEntityType.DeferredOperationStatus;
                    retrieveOperationStatus.InstanceTag = new EntityInstanceId { Id = operationId };

                    DeferredOperationStatus deferredOperationStatus;

                    //  _upgradeLog.WriteInformation("Retrieving state of the create organisation job...");

                    // Retrieve the Operation State until Organization is created
                    do
                    {
                        // Wait 3 secs to not overload server
                        Thread.Sleep(3000);
                        var retrieveResponse = (RetrieveResponse)service.Execute(retrieveOperationStatus);
                        deferredOperationStatus = ((DeferredOperationStatus)retrieveResponse.Entity);
                    }
                    while (deferredOperationStatus.State != DeferredOperationState.Processing
                        && deferredOperationStatus.State != DeferredOperationState.Completed);

                    // Poll OrganizationStatusRequest
                    var retrieveReqServer = new RetrieveRequest();
                    retrieveReqServer.EntityType = DeploymentEntityType.Organization;
                    retrieveReqServer.InstanceTag = new EntityInstanceId();
                    retrieveReqServer.InstanceTag.Name = org.UniqueName;

                    Microsoft.Xrm.Sdk.Deployment.OrganizationState orgState;

                    upgradeLog.WriteInformation("Retrieving state of the organization...");

                    // Retrieve and check the Organization State until is enabled
                    RetrieveResponse resp = null;
                    do
                    {
                        resp = (RetrieveResponse)service.Execute(retrieveReqServer);
                        //  orgId = ((Microsoft.Xrm.Sdk.Deployment.Organization)retrieveRespServer.Entity).Id;
                        orgState = ((Organization)resp.Entity).State;
                        // Wait 5 secs to not overload server
                        Thread.Sleep(5000);
                    }
                    while (orgState != OrganizationState.Enabled && orgState != OrganizationState.Failed);
                    if (orgState == OrganizationState.Enabled)
                    {
                        upgradeLog.WriteInformation("Organization has been created!");
                    }
                    else
                    {
                        upgradeLog.WriteInformation("The organization create operation failed.!");
                        throw new Exception("The organization create operation failed.!");
                    }

                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                upgradeLog.WriteError("The application encountered an error.");
                upgradeLog.WriteError("Timestamp: {0}", ex.Detail.Timestamp);
                upgradeLog.WriteError("Code: {0}", ex.Detail.ErrorCode);
                upgradeLog.WriteError("Message: {0}", ex.Detail.Message);
                upgradeLog.WriteError("Plugin Trace: {0}", ex.Detail.TraceText);
                upgradeLog.WriteError("Inner Fault: {0}",
                    null == ex.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                throw;
            }
            catch (TimeoutException ex)
            {
                upgradeLog.WriteError("The application encountered an error..");
                upgradeLog.WriteError("Message: {0}", ex.Message);
                upgradeLog.WriteError("Stack Trace: {0}", ex.StackTrace);
                upgradeLog.WriteError("Inner Fault: {0}", string.IsNullOrEmpty(ex.InnerException.Message) ? "No Inner Fault" : ex.InnerException.Message);
                throw;
            }
            catch (Exception ex)
            {
                upgradeLog.WriteError("The application encountered  an error.");
                upgradeLog.WriteError(ex.Message);

                // Display the details of the inner exception.
                if (ex.InnerException != null)
                {
                    upgradeLog.WriteError(ex.InnerException.Message);

                    var fe = ex.InnerException as FaultException<OrganizationServiceFault>;
                    if (fe != null)
                    {
                        upgradeLog.WriteError("Timestamp: {0}", fe.Detail.Timestamp);
                        upgradeLog.WriteError("Code: {0}", fe.Detail.ErrorCode);
                        upgradeLog.WriteError("Message: {0}", fe.Detail.Message);
                        upgradeLog.WriteError("Plugin Trace: {0}", fe.Detail.TraceText);
                        upgradeLog.WriteError("Inner Fault: {0}",
                            null == fe.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                    }
                }
                throw;
            }
            // Additional exceptions to catch: SecurityTokenValidationException, ExpiredSecurityTokenException,
            // SecurityAccessDeniedException, MessageSecurityException, and SecurityNegotiationException.

        }
    }
}