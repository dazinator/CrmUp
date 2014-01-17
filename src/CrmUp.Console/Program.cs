﻿using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using DbUp;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk.Deployment;

namespace CrmUp
{
    class Program
    {
        static int Main(string[] args)
        {

            var connectionStringArgument = args.FirstOrDefault();
            bool isKey = false;
            if (string.IsNullOrEmpty(connectionStringArgument))
            {
                isKey = true;
                connectionStringArgument = "CrmConnectionString";
            }

            string connString = string.Empty;
            if (isKey)
            {
                var connStringSetting = ConfigurationManager.ConnectionStrings[connectionStringArgument];
                if (connStringSetting == null)
                {
                    throw new ArgumentException("Connection string not found in config file with name:" + connectionStringArgument);
                }
                connString = connStringSetting.ConnectionString;
            }
            else
            {
                connString = connectionStringArgument;
            }

            // Ensure the Crm Organisation exists. Get Crm Organisation name from connection string.
            var crmConnection = CrmConnection.Parse(connString);
            EnsureOrganisation(crmConnection);

            var upgrader =
               DeployChanges.To
                            .DynamicsCrm(connectionString, isKey)
                            .WithSolutionsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                            .LogToConsole()
                            .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Success!");
            Console.ResetColor();
            return 0;

        }

        private static void EnsureOrganisation(CrmConnection crmConnection, string orgName, string orgFriendlyName, string sysAdminName)
        {

            Guid orgId;
            string sqlServerName = string.Empty;
            string srsUrl = string.Empty;
            try
            {

                using (var deploymentService = new DeploymentService(crmConnection))
                {
                    // Set properties for the new organization
                    //TODO: Deserialise from xml file?
                    var organization = new Microsoft.Xrm.Sdk.Deployment.Organization
                    {
                        BaseCurrencyCode = "GBP",
                        BaseCurrencyName = "Pound Sterling",
                        BaseCurrencyPrecision = 2,
                        BaseCurrencySymbol = "£",
                        BaseLanguageCode = 1033,
                        FriendlyName = orgFriendlyName,
                        UniqueName = orgName,
                        SqlCollation = "Latin1_General_CI_AI",
                        SqlServerName = sqlServerName,
                        SrsUrl = srsUrl,
                        SqmIsEnabled = false
                    };

                    var request = new BeginCreateOrganizationRequest
                    {
                        Organization = organization,
                        SysAdminName = sysAdminName
                    };

                    // Execute the request
                    var response = (BeginCreateOrganizationResponse)deploymentService.Execute(request);

                    // The operation is asynchronous, so the response object contains
                    // a unique identifier for the operation
                    Guid operationId = response.OperationId;

                    // Retrieve the Operation using the OperationId
                    var retrieveOperationStatus = new RetrieveRequest();
                    retrieveOperationStatus.EntityType = DeploymentEntityType.DeferredOperationStatus;
                    retrieveOperationStatus.InstanceTag = new EntityInstanceId { Id = operationId };

                    DeferredOperationStatus deferredOperationStatus;

                    Console.WriteLine("Retrieving state of the job...");

                    // Retrieve the Operation State until Organization is created
                    do
                    {
                        // Wait 3 secs to not overload server
                        Thread.Sleep(3000);

                        var retrieveResponse = (RetrieveResponse)deploymentService.Execute(retrieveOperationStatus);

                        deferredOperationStatus = ((DeferredOperationStatus)retrieveResponse.Entity);
                    }
                    while (deferredOperationStatus.State != DeferredOperationState.Processing
                        && deferredOperationStatus.State != DeferredOperationState.Completed);

                    // Poll OrganizationStatusRequest
                    var retrieveReqServer = new RetrieveRequest();
                    retrieveReqServer.EntityType = DeploymentEntityType.Organization;
                    retrieveReqServer.InstanceTag = new EntityInstanceId();
                    retrieveReqServer.InstanceTag.Name = organization.UniqueName;

                    OrganizationState orgState;

                    Console.WriteLine("Retrieving state of the organization...");

                    // Retrieve and check the Organization State until is enabled
                    do
                    {
                        var retrieveRespServer = (RetrieveResponse)deploymentService.Execute(retrieveReqServer);
                        orgId = ((Microsoft.Xrm.Sdk.Deployment.Organization)retrieveRespServer.Entity).Id;
                        orgState = ((Microsoft.Xrm.Sdk.Deployment.Organization)retrieveRespServer.Entity).State;

                        // Wait 5 secs to not overload server
                        Thread.Sleep(5000);
                    }
                    while (orgState != OrganizationState.Enabled);

                    Console.WriteLine("Organization has been created!");
                }
            }
            catch (FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> ex)
            {
                Console.WriteLine("The application encountered an error.");
                Console.WriteLine("Timestamp: {0}", ex.Detail.Timestamp);
                Console.WriteLine("Code: {0}", ex.Detail.ErrorCode);
                Console.WriteLine("Message: {0}", ex.Detail.Message);
                Console.WriteLine("Plugin Trace: {0}", ex.Detail.TraceText);
                Console.WriteLine("Inner Fault: {0}",
                    null == ex.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
            }
            catch (System.TimeoutException ex)
            {
                Console.WriteLine("The application encountered an error..");
                Console.WriteLine("Message: {0}", ex.Message);
                Console.WriteLine("Stack Trace: {0}", ex.StackTrace);
                Console.WriteLine("Inner Fault: {0}",
                    null == ex.InnerException.Message ? "No Inner Fault" : ex.InnerException.Message);
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("The application encountered  an error.");
                Console.WriteLine(ex.Message);

                // Display the details of the inner exception.
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);

                    var fe = ex.InnerException as FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>;
                    if (fe != null)
                    {
                        Console.WriteLine("Timestamp: {0}", fe.Detail.Timestamp);
                        Console.WriteLine("Code: {0}", fe.Detail.ErrorCode);
                        Console.WriteLine("Message: {0}", fe.Detail.Message);
                        Console.WriteLine("Plugin Trace: {0}", fe.Detail.TraceText);
                        Console.WriteLine("Inner Fault: {0}",
                            null == fe.Detail.InnerFault ? "No Inner Fault" : "Has Inner Fault");
                    }
                }
            }
            // Additional exceptions to catch: SecurityTokenValidationException, ExpiredSecurityTokenException,
            // SecurityAccessDeniedException, MessageSecurityException, and SecurityNegotiationException.

            finally
            {
                // Console.WriteLine("Press <Enter> to exit.");
                // Console.ReadLine();
            }
        }
    }
}