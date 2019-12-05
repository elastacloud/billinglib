using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsLib
{
    public abstract class DataMetric
    {
        public double Average { protected set; get; }
        public double Maximum { protected set; get; }
        public double Minimum { protected set; get; }
        public double Total { protected set; get; }
        public abstract string MetricName { get; }
        public void SetMetrics(double average, double maximum, double minimum, double total)
        {
            Average = average;
            Maximum = maximum;
            Minimum = minimum;
            Total = total;
        }
    }
}
