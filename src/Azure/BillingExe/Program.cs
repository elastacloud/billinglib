using BillingLib;
using MetricsLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BillingExe
{
   class Program
   {
      static async Task Main(string[] args)
      {
         var container = BillingInvoker.GetCustomerContainer();
         if (container.EventHubEntity == "billingdemo")
         {
            Console.WriteLine("Starting billing extraction");
            await GetLastHourBilling();
            Console.WriteLine("Ending billing extraction");
         }
         else
         {
            Console.WriteLine("Starting metrics extraction");
            var metrics = await GetMetrics();
            Console.WriteLine("Ending metrics extraction");
         }
         Console.WriteLine("Press any key ...");
         Console.Read();
      }

      public static async Task GetLastHourBilling()
      {
         var invoker = new BillingInvoker();
         await invoker.PopulateRecentRateAndUsageInformation(BillingInvoker.GetCustomerContainer());
      }

      public static async Task<List<DatabaseMetric>> GetMetrics()
      {
         var metrics = new DatabaseMetrics();
         return await metrics.GetMetrics(BillingInvoker.GetCustomerContainer());
      }
   }
}
