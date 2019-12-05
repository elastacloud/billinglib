using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using AadHelper;
using Newtonsoft.Json;

namespace BillingLib
{
    public class UsageBillingRequest : IBillingRequest
    {
        public async Task<string> MakeRequest(CustomerContainer container)
        {
            // configure the start time usage first of all and then end time in ISO8601 - time wi
            string usageStartTime = container.StartTime.ToString("yyyy-MM-ddTHH:00:00Z").Replace(":", "%3a");
            string usageEndTime = container.EndTime.ToString("yyyy-MM-ddTHH:00:00Z").Replace(":", "%3a");

            string payload = null;
            var issuer = new TokenIssuer();
            // Build up the HttpWebRequest 
            string requestUrl =
                $"{container.ArmBillingServiceUrl}/subscriptions/{container.SubscriptionId}/providers/Microsoft.Commerce/UsageAggregates?api-version=2015-06-01-preview&reportedstartTime={usageStartTime}&reportedEndTime={usageEndTime}&aggregationGranularity=Hourly&showDetails=true";
            var newRequest = container.RequestNextLink ?? requestUrl;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(newRequest);

            // Add the OAuth Authorization header, and Content Type header 
            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + await issuer.GetOAuthTokenFromAad(container));
            request.ContentType = "application/json";
            // TODO: check to see that a valid response code is being returned here...
            using (var response = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                payload = await response.ReadToEndAsync();
            }
            
            return payload;
        }

        public async Task<T> MakeObjectRequest<T>(CustomerContainer container)
        {
            var payload = await MakeRequest(container);
            // read this into a JSON string we're going to get each next link
            var conversion = JsonConvert.DeserializeObject<Usage>(payload);
            var nextPayload = new NextPayload(conversion.NextLink, conversion.Value);
            // crappy but works - boxing/unboxing get rid of 
            return (T)(object)nextPayload;
        }   
    }

    public class NextPayload
    {
        public NextPayload(string nextLink, List<UsageAggregate> payload)
        {
            NextLink = nextLink;
            Payload = payload;
        }
        public string NextLink { get; }
        public List<UsageAggregate> Payload { get; }
    }
}