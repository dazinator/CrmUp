using System;
using System.Configuration;
using System.Xml;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using NUnit.Framework;

namespace CrmUp.Tests.Sample_Code
{
    [TestFixture]
    class UtilityTests
    {
        
        [Test]
        public void Get_Sample_Organisation_Entity_And_Write_To_Xml()
        {

            // Ensure the Crm Organisation exists. Get Crm Organisation name from connection string.
            var connProvider = new ExplicitConnectionStringProviderWithFallbackToConfig();
            var crmServiceProvider = new CrmServiceProvider(connProvider, new CrmClientCredentialsProvider());
            var orgService = crmServiceProvider.GetOrganisationService();
            using (orgService as IDisposable)
            {
                var results = orgService.RetrieveMultiple(new QueryExpression("organization") { ColumnSet = new ColumnSet(true) });
                foreach (var result in results.Entities)
                {
                    Console.Write(result.Serialize(Formatting.Indented));
                    Console.WriteLine();
                }
            }


        }


    }
}