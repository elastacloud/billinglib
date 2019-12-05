using System;

namespace BillingLib
{
   public class CustomerData
   {
      public string SubscriptionId { get; set; }
      public string MeterId { get; set; }
      public string MeterName { get; set; }
      public string Category { get; set; }
      public string Unit { get; set; }
      public double Price { get; set; }
      public double Quantity { get; set; }
      public double Cost { get; set; }
      public DateTime StartTime { get; set; }
      public DateTime EndTime { get; set; }
      public string ResourceGroup { get; set; }
      public DateTime ReportedStartDate { get; set; }
      public DateTime ReportedEndDate { get; set; }
      public string ResourceName { get; set; }
      public string ResourceInstanceName { get; set; }
      public string ResourceProvider { get; set; }
      public string Tags { get; set; }
      public string Location { get; set; }
   }

}  
