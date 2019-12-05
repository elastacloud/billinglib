using System;
using System.Collections.Generic;
using System.Text;

namespace MetricsLib
{
   public class RawDataMetric
   {
      public double Average { set; get; }
      public double Maximum { set; get; }
      public double Minimum { set; get; }
      public double Total { set; get; }
      public string MetricName { get; set; }
      public string ServerName { get; set; }
      public string DatabaseName { get; set; }
      public DateTime StartCollectionTime { get; set; }
      public DateTime EndCollectionTime { get; set; }
      public string ServiceType { get; set; }
   }
}
