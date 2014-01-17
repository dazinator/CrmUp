using System;
using System.Linq;
using System.Reflection;
using DbUp;

namespace CrmUp
{
    class Program
    {
        static int Main(string[] args)
        {

            var connectionString = args.FirstOrDefault();
            bool isKey = false;
            if (string.IsNullOrEmpty(connectionString))
            {
                isKey = true;
                connectionString = "CrmConnectionString";
            }
         
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
    }
}
