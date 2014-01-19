using System;
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
            var upgrader =
               DeployChanges.To
                            .DynamicsCrmOrganisation()
                            .WithSolutionsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                            .CreateIfDoesNotExist(OrgToCreate)
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

        private static Organization OrgToCreate()
        {
            return new Microsoft.Xrm.Sdk.Deployment.Organization
                {
                    BaseCurrencyCode = "GPB",
                    BaseCurrencyName = "Pound Sterling",
                    BaseCurrencyPrecision = 2,
                    BaseCurrencySymbol = "£",
                    BaseLanguageCode = 1033,
                    FriendlyName = "CrmUpTest",
                    UniqueName = "CrmUpTest",
                    SqlCollation = "Latin1_General_CI_AI",
                    SqlServerName = "VM-PRODDEV-SQL",
                    SrsUrl = "http://VM-PRODDEV-SQL/reportserver",
                    SqmIsEnabled = false
                };
        }

    }
}
