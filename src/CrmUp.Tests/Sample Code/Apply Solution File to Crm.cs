﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using DbUp;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Client;

namespace CrmUp.Tests.Sample_Code
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var upgrader =
               DeployChanges.To
                            .DynamicsCrmOrganisation("someconnectionstring")
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
    }
}
