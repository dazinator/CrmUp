﻿using System;
using System.Configuration;
using System.Globalization;
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

        private static CreateOrganisationArgs OrgToCreate()
        {
            var args = new CreateOrganisationArgs();
            args.Organisation = new Microsoft.Xrm.Sdk.Deployment.Organization
                {
                    BaseCurrencyCode = RegionInfo.CurrentRegion.ISOCurrencySymbol,
                    BaseCurrencyName = RegionInfo.CurrentRegion.CurrencyNativeName,
                    BaseCurrencyPrecision = 2,
                    BaseCurrencySymbol = RegionInfo.CurrentRegion.CurrencySymbol,
                    BaseLanguageCode = 1033,
                    FriendlyName = ConfigurationManager.AppSettings["CrmOrganisationName"],
                    UniqueName = ConfigurationManager.AppSettings["CrmOrganisationName"],
                    SqlCollation = "Latin1_General_CI_AI",
                    SqlServerName = ConfigurationManager.AppSettings["CrmSqlServer"],
                    SrsUrl = ConfigurationManager.AppSettings["CrmSSRSServer"],
                    SqmIsEnabled = false
                };
            args.SystemAdmingUser = ConfigurationManager.AppSettings["CrmSystemAdminUserAccount"];
            return args;
        }

    }
}
