using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AadHelper;
using Microsoft.Azure;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.Management.Sql;
using Microsoft.Azure.Management.Sql.Models;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MetricsLib
{
    public class DatabaseMetrics
    {
      private CustomerContainer _container;
        // https://management.azure.com/subscriptions/{subscription-id}/resourceGroups/{resource-group-name}/providers/Microsoft.Sql/servers/{server-name}/databases/{database-name}/metrics?api-version={api-version}&$filter={metrics-filter}
        public async Task<List<DatabaseMetric>> GetMetrics(CustomerContainer container)
        {
         const string urlSqlDb = "(name / value eq 'blocked_by_firewall' or name / value eq 'connection_failed' or name / value eq 'connection_successful' or name / value eq 'cpu_percent' or name / value eq 'deadlock' or name / value eq 'dtu_consumption_percent' or name / value eq 'log_write_percent' or name / value eq 'log_write_percent' or name / value eq 'physical_data_read_percent' or name / value eq 'storage' or name / value eq 'storage_percent' or name / value eq 'workers_percent' or name / value eq 'sessions_percent' or name / value eq 'dtu_limit' or name / value eq 'dtu_used' or name / value eq 'dwu_limit' or name / value eq 'dwu_used' or name / value eq 'dwu_consumption_percent') and timeGrain eq '00:00:15'";
         var allDataMetrics = new List<DatabaseMetric>();
         _container = container;
         var login = new AzureLogin(container);
         var token = await login.AcquireToken(login.Audience);
         var tokenCredentials = new TokenCredentials(token);
         var client = new SqlManagementClient(tokenCredentials)
         {
            SubscriptionId = container.SubscriptionId
         };
         var rgs = await login.GetSubscriptionsResourceGroups(login.GetSubscriptions);
         var subs = rgs.Where(subsrgs => subsrgs.Subscription.SubscriptionId == container.SubscriptionId).Select(rgs1 => rgs1.ResourceGroup);
         foreach (var rg in subs)
         {
            foreach (var server in client.Servers.ListByResourceGroup(rg))
            {
               foreach (var db in await client.Databases.ListByServerAsync(rg, server.Name))
               {
                  var dbMetrics = new List<DataMetric>(new DataMetric[] { new DtuLimit(),
                           new DtuUsed(), new DtuConsumptionPercentage(), new DwuLimit(),
                           new DwuUsed(), new DwuConsumptionPercentage(), new BlockedByFirewall(),
                           new CpuPercent(), new FailedConnections(), new SuccessfulConnections() });
                  
                  var filteredDb = await GetMetricsQueryResponse(container.SubscriptionId,
                     container.RunReportStartTime.Value, container.RunReportEndTime.Value, 
                     db.Name, rg, server.Name, login, token, urlSqlDb, dbMetrics, DataSourceType.SqlDb);
                  if (filteredDb == null)
                     continue;
                  allDataMetrics.Add(filteredDb);
               }
            }
         }
         
         return allDataMetrics;
      }

      private async Task<DatabaseMetric> GetMetricsQueryResponse(string subscriptionId,
         DateTime start,
         DateTime end,
         string db,
         string rg,
          string server,
          AzureLogin login,
          string token,
          string filter,
          List<DataMetric> metrics,
          DataSourceType type)
      {
         if (db == "master")
            return null;

         var connectionStringBuilder = new EventHubsConnectionStringBuilder(_container.EventHubConnectionString)
         {
            EntityPath = _container.EventHubEntity
         };
         var eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
         var metricOut = new DatabaseMetric(db, server, start, end, type);
         string url =
             $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{rg}/providers/Microsoft.Sql/servers/{server}/databases/{db}/metrics";
         string startString = start.ToString("yyyy-MM-dd HH:mm:ss");
         string endString = end.ToString("yyyy-MM-dd HH:mm:ss");
         string oDataFilter =
             $"{filter} and startTime eq '{startString}' and endTime eq '{endString}'";
         var filtered =
             await login.GetJObjectWithFilter(url, token, oDataFilter, "2014-04-01-Preview");
         foreach (var node in filtered)
         {
            string localName = ((JToken)node["name"])["value"].Value<string>();
            foreach (var metric in metrics)
            {
               if (localName == metric.MetricName)
               {
                  var jsonOut = JsonConvert.SerializeObject(new RawDataMetric()
                  {
                     Average = ((JToken)node["metricValues"]).Average(val => val["average"].Value<double>()),
                     Maximum = ((JToken)node["metricValues"]).Max(val => val["maximum"].Value<double>()),
                     Minimum = ((JToken)node["metricValues"]).Min(val => val["minimum"].Value<double>()),
                     Total = ((JToken)node["metricValues"]).Sum(val => val["total"].Value<double>()),
                     DatabaseName = db,
                     ServerName = server,
                     MetricName = metric.MetricName,
                     ServiceType = Enum.GetName(typeof(DataSourceType), GetCorrectDataSourceType(localName)),
                     StartCollectionTime = start,
                     EndCollectionTime = end
                  });

                  await eventHubClient.SendAsync(new EventData(Encoding.UTF8.GetBytes(jsonOut)));
               }
            }
         }
         await eventHubClient.CloseAsync();
         return metricOut.DataMetrics.Count == 0 ? null : metricOut;
      }

        private DataSourceType GetCorrectDataSourceType(string metricName)
        {
            if (metricName.StartsWith("dwu"))
            {
                return DataSourceType.SqlDw;
            }
            return DataSourceType.SqlDb;
        }
    }
}
