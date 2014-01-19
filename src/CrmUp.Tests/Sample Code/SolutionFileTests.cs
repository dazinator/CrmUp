using System;
using System.Configuration;
using System.Xml;
using DbUp.Engine.Transactions;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using NUnit.Framework;

namespace CrmUp.Tests.Sample_Code
{
    [TestFixture]
    class UtilityTests
    {

        //[Test]
        //public void Get_Sample_Organisation_Entity_And_Write_To_Xml()
        //{

        //    // Ensure the Crm Organisation exists. Get Crm Organisation name from connection string.
        //    var connProvider = new ExplicitConnectionStringProviderWithFallbackToConfig();
        //    var crmServiceProvider = new CrmServiceProvider(connProvider, new CrmClientCredentialsProvider());
        //    var orgService = crmServiceProvider.GetOrganisationService();
        //    using (orgService as IDisposable)
        //    {
        //        var orgsSer = orgService as OrganizationServiceContext;
        //        var orgs = orgService.e
        //        var results = orgService.RetrieveMultiple(new QueryExpression("organization") { ColumnSet = new ColumnSet(true) });
        //        foreach (var result in results.Entities)
        //        {
        //            Console.Write(result.Serialize(Formatting.Indented));
        //            Console.WriteLine();
        //        }
        //    }


        //}

        //[Test]
        //public void Check_For_Journal_Entity()
        //{

        //    var retrieveJournalEntityRequest = new RetrieveEntityRequest
        //        {
        //            EntityFilters = EntityFilters.Entity,
        //            LogicalName = CrmEntityJournal.JournalEntityName
        //        };




        //    // Ensure the Crm Organisation exists. Get Crm Organisation name from connection string.
        //    var connProvider = new ExplicitConnectionStringProviderWithFallbackToConfig();
        //    var crmServiceProvider = new CrmServiceProvider(connProvider, new CrmClientCredentialsProvider());
        //    var orgService = crmServiceProvider.GetOrganisationService();
        //    using (orgService as IDisposable)
        //    {
        //        try
        //        {
        //            var response = (RetrieveEntityResponse)orgService.Execute(retrieveJournalEntityRequest);
        //            if (response == null)
        //            {

        //            }
        //        }
        //        catch (System.ServiceModel.FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> fault)
        //        {
        //            if (fault.Message == "Could not find entity")
        //            {

        //            }
        //            throw;
        //        }

        //    }



        //    //return false;
        //}



    }
}