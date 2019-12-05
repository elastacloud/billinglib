using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetricsLib
{
    public class DatabaseMetric
    {
        public DatabaseMetric(string databaseName, string databaseServerName, DateTime startDate, DateTime endDate, DataSourceType type)
        {
            DatabaseName = databaseName;
            DatabaseServerName = databaseServerName;
            StartTime = startDate;
            EndTime = endDate;
            DataSourceType = type;
            DataMetrics = new List<DataMetric>();
        }
        public string DatabaseName { get; private set; }
        public string DatabaseServerName { get; private set; }
        public List<DataMetric> DataMetrics { get; set; }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public DataSourceType DataSourceType { get; set; }
    }
}
