using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AadHelper;
using BillingLib;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;

namespace BillingLib
{
   public class BillingInvoker
   {
      private string _eventHubName;
      private string _eventConnectionString;

      public BillingInvoker()
      {
         _eventHubName = ConfigurationManager.AppSettings["EventHubName"];
         _eventConnectionString = ConfigurationManager.AppSettings["EventHubConnectionString"];
      }

      public BillingInvoker(string eventHubName, string eventHubConnectionString)
      {
         _eventHubName = eventHubName;
         _eventConnectionString = eventHubConnectionString;
      }

      /// <summary>
      /// Gets a list of customer subscriptions to run with 
      /// </summary>
      public static CustomerContainer GetCustomerContainer()
      {
         return new CustomerContainer()
         {
            Currency = ConfigurationManager.AppSettings["Currency"],
            RegionInfo = ConfigurationManager.AppSettings["RegionInfo"],
            SubscriptionId = ConfigurationManager.AppSettings["SubscriptionId"],
            ClientId = ConfigurationManager.AppSettings["client_Id"],
            ClientKey = ConfigurationManager.AppSettings["client_key"],
            TenantDomain = ConfigurationManager.AppSettings["tenant"],
            OfferId = ConfigurationManager.AppSettings["OfferId"],
            AdalServiceUrl = ConfigurationManager.AppSettings["ADALServiceURL"],
            ArmBillingServiceUrl = ConfigurationManager.AppSettings["ARMBillingServiceURL"],
            EventHubEntity = ConfigurationManager.AppSettings["EventHubName"],
            EventHubConnectionString = ConfigurationManager.AppSettings["EventHubConnectionString"]
         };
      }

      /// <summary>
      /// Iterates through the customers and populates their data into SQL
      /// </summary>
      public async Task PopulateRecentRateAndUsageInformation(CustomerContainer container)
      {
         var connectionStringBuilder = new EventHubsConnectionStringBuilder(_eventConnectionString)
         {
            EntityPath = _eventHubName
         };
         var eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
         var now = DateTime.UtcNow;
         //Set the sart and end dates to pull data into the db
         var lastUpdateReportedStart = DateTime.UtcNow.Subtract(TimeSpan.FromHours(6));
         var reportEndDate = DateTime.UtcNow.Subtract(TimeSpan.FromHours(5));
         var usage = new AzureUsage(container);
         var rates =
               await usage.GetCombinedRatesAndUsage(lastUpdateReportedStart, reportEndDate, container.OfferId);

         int index = 0;
         int count = rates.Value.Count;
         var customerDatas = new List<CustomerData>();

         foreach (var rate in rates.Value)
         {
            string jsonOut = JsonConvert.SerializeObject(new CustomerData()
            {
               Category = rate.Properties.MeterCategory,
               MeterId = rate.Properties.MeterId,
               Price = rate.Properties.Price,
               Cost = rate.Properties.Cost,
               Unit = rate.Properties.Unit,
               Quantity = rate.Properties.Quantity,
               MeterName = rate.Properties.MeterName,
               StartTime = DateTime.Parse(rate.Properties.UsageStartTime),
               EndTime = DateTime.Parse(rate.Properties.UsageEndTime),
               SubscriptionId = container.SubscriptionId,
               ResourceGroup = rate.Properties.ResourceGroup,
               ReportedStartDate = rates.ReportedStartDate,
               ReportedEndDate = rates.ReportedEndData,
               ResourceProvider = rate.Properties.ResourceProvider,
               ResourceName = rate.Properties.ResourceName,
               ResourceInstanceName = rate.Properties.ResourceSubName,
               Tags = rate.Properties.Tags,
               Location = rate.Properties.Location
            });
            await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(jsonOut)));
            Console.WriteLine($"Adding record {++index} of {count} ");
         }
         await eventHubClient.CloseAsync();
      }
   }

}  
