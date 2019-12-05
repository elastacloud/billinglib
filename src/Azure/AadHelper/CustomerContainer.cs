using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AadHelper
{
   public class CustomerContainer
   {
      public string Name { get; set; }
      public string ClientId { get; set; }
      public string ClientKey { get; set; }
      public string ResourceGroup { get; set; }
      public string Location { get; set; }
      public string SubscriptionId { get; set; }
      public string TenantDomain { get; set; }
      public string OfferId { get; set; }
      public DateTime StartTime { get; set; }
      public DateTime EndTime { get; set; }
      public string RequestNextLink { get; set; }
      public string StorageAccountName { get; set; }
      public string QueueSASToken { get; set; }
      public string WebAppName { get; set; }
      public string WebAppPubUser { get; set; }
      public string WebAppPubPassword { get; set; }
      public int SubscriptionIndex { get; set; }
      public string Currency { get; set; }
      public string RegionInfo { get; set; }
      public string ArmBillingServiceUrl { get; set; }
      public string AdalServiceUrl { get; set; }
      public string GraphApiUrl { get; set; }
      public string MicrosoftLoginUrl { get; set; }
      public string EventHubConnectionString { get; set; }
      public string EventHubEntity { get; set; }
      public DateTime? RunReportStartTime { get { return DateTime.UtcNow.Subtract(TimeSpan.FromHours(6)); } }
      public DateTime? RunReportEndTime { get { return DateTime.UtcNow.Subtract(TimeSpan.FromHours(5));  } }
   }
}
