using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsLib
{
    public class DatabaseUsageMetric
    {
        public string DatabaseName { get; set; }
        public string DatabaseServerName { get; set; }
        public double MetricPercentage { get; set; }
        public double MetricLimit { get; set; }
        public double MetricUsedMaximum { get; set; }
        public double MetricUsedMinimum { get; set; }
        public double MetricUsedAverage { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }

    public class DtuMetric : DatabaseUsageMetric
    {
        public const string MetricType = "DTU";
    }

    public class DwuMetric : DatabaseUsageMetric
    {
        public const string MetricType = "DWU";
    }
}
