using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;

namespace AadHelper
{
    public class AzureLogin : IAzureLogin
    {
      private CustomerContainer _container;

      public AzureLogin(CustomerContainer container)
      {
         _container = container;
      }
      public string Audience => _container.ArmBillingServiceUrl;

        public async Task<string> AcquireToken(string audience)
        {
            var creds = new ClientCredential(_container.ClientId, _container.ClientKey);
            var ac =
                new AuthenticationContext($"{_container.AdalServiceUrl}/{_container.TenantDomain}");
            var ar = await ac.AcquireTokenAsync($"{audience}/", creds);
            return ar.AccessToken;
        }

        public async Task<JArray> GetJObject(string url, string token, string apiVersion = "2015-03-20")
        {
            var request = WebRequest.Create($"{url}?api-version={apiVersion}");
            request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {token}");
            var response = await request.GetResponseAsync();
            var responseStream = response.GetResponseStream();
            JObject jObject = null;

            using (var reader = new StreamReader(responseStream))
            {
                string jsonResponse = await reader.ReadToEndAsync();
                jObject = JObject.Parse(jsonResponse);
            }

            return jObject["value"] as JArray;
        }

        public async Task<JArray> GetJObjectWithFilter(string url, string token, string filter, string apiVersion = "2015-03-20")
        {
            var request = WebRequest.Create($"{url}?api-version={apiVersion}&$filter={filter}");
            request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {token}");
            var response = await request.GetResponseAsync();
            var responseStream = response.GetResponseStream();
            JObject jObject = null;

            using (var reader = new StreamReader(responseStream))
            {
                string jsonResponse = await reader.ReadToEndAsync();
                jObject = JObject.Parse(jsonResponse);
            }

            return jObject["value"] as JArray;
        }

        public async Task<JArray> GetJObjectPost(string url, string token, string query, string apiVersion = "2015-03-20")
        {
            var request = WebRequest.Create($"{url}?api-version={apiVersion}");
            request.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {token}");
            request.ContentType = "application/json";
            request.Method = "POST";
            using (var writer = new StreamWriter(request.GetRequestStream()))
            {
                await writer.WriteAsync(query);
            }
            var response = await request.GetResponseAsync();
            var responseStream = response.GetResponseStream();
            JObject jObject = null;
            string jsonResponse = null;

            using (var reader = new StreamReader(responseStream))
            {
                jsonResponse = await reader.ReadToEndAsync();
                jObject = JObject.Parse(jsonResponse);
            }

            return jObject["value"] as JArray;
        }

        public async Task<List<SubscriptionResourceGroup>> GetSubscriptionsResourceGroups(Func<Task<List<Subscription>>> getSubscriptions)
        {
            var subscriptions = await getSubscriptions.Invoke();
            var subscriptionResourceGroups = new List<SubscriptionResourceGroup>();
            string audience = _container.ArmBillingServiceUrl;
            string token = await AcquireToken(audience);

            try
            {
                foreach (var subscription in subscriptions)
                {
                    var jObject = await GetJObject($"{audience}/subscriptions/{subscription.SubscriptionId}/resourceGroups", token, "2016-06-01");

                    subscriptionResourceGroups.AddRange(jObject.Select(obj => new SubscriptionResourceGroup
                    {
                        Subscription = subscription,
                        ResourceGroup = (string)obj["name"]
                    }));
                }
            }
            catch (Exception ex)
            {
                // this will need to removed and we need to put logging in 
                throw ex;
            }
            return subscriptionResourceGroups;
        }

        /// <summary>
        /// Gets a list of subscriptions in the AAD
        /// </summary>
        public async Task<List<Subscription>> GetSubscriptions()
        {
            var subscriptions = new List<Subscription>();
            string audience = _container.ArmBillingServiceUrl;
            string token = await AcquireToken(audience);

            try
            {
                var jObject = await GetJObject($"{audience}/subscriptions", token);

                subscriptions.AddRange(jObject.Select(obj => new Subscription
                {
                    DisplayName = (string)obj["displayName"],
                    SubscriptionId = (string)obj["subscriptionId"]
                }));
            }
            catch (Exception ex)
            {
                // this will need to removed and we need to put logging in 
                throw ex;
            }
            return subscriptions;
        }
    }

    public interface IAzureLogin
    {
        Task<List<Subscription>> GetSubscriptions();
        Task<JArray> GetJObject(string url, string token, string apiVersion = "2015-03-20");
        Task<JArray> GetJObjectPost(string url, string token, string query, string apiVersion = "2015-03-20");

        Task<List<SubscriptionResourceGroup>> GetSubscriptionsResourceGroups(
            Func<Task<List<Subscription>>> getSubscriptions);

        Task<string> AcquireToken(string audience);
    }
}
