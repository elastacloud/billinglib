using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using BillingLib;
using Microsoft.Extensions.Configuration;
using AadHelper;

namespace BillingFunction
{
    public static class BillingFunctions
    {
      [FunctionName("BillingFunction")]
      public async static Task Run([TimerTrigger("0 0 * * * *")]TimerInfo myTimer, TraceWriter log, ExecutionContext context)
      {
         var config = new ConfigurationBuilder()
             .SetBasePath(context.FunctionAppDirectory)
             .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
             .AddEnvironmentVariables()
             .Build();

         string eventHubName = config["EventHubName"];
         string eventHubConnectionString = config["EventHubConnectionString"];

         var container = new CustomerContainer()
         {
            Currency = config["Currency"],
            RegionInfo = config["RegionInfo"],
            SubscriptionId = config["SubscriptionId"],
            ClientId = config["client_Id"],
            ClientKey = config["client_key"],
            TenantDomain = config["tenant"],
            OfferId = config["OfferId"],
            ArmBillingServiceUrl = config["ARMBillingServiceURL"],
            AdalServiceUrl = config["ADALServiceURL"]
         };

         var invoker = new BillingInvoker(eventHubName, eventHubConnectionString);
         await invoker.PopulateRecentRateAndUsageInformation(container);
      }
    }
}
