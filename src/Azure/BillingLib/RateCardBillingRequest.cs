using AadHelper;
using System.Configuration;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace BillingLib
{
    public class RateCardBillingRequest : IBillingRequest
    {
        public async Task<string> MakeRequest(CustomerContainer container)
        {
            string payload = null;
            var issuer = new TokenIssuer();
            // Build up the HttpWebRequest 
            string requestUrl =
                $"{container.ArmBillingServiceUrl}/subscriptions/{container.SubscriptionId}/providers/Microsoft.Commerce/RateCard?api-version=2015-06-01-preview&$filter=OfferDurableId eq '{container.OfferId}' and Currency eq '{container.Currency}' and Locale eq 'en-GB' and RegionInfo eq '{container.RegionInfo}'";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);

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

        public Task<T> MakeObjectRequest<T>(CustomerContainer container)
        {
            throw new System.NotImplementedException();
        }
    }
}