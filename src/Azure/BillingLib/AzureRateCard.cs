using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AadHelper;
using Newtonsoft.Json;

namespace BillingLib
{
    /// <summary>
    /// Returns the details of an Azure ratecard based on the offer id that is sent - currently hard coded to query in GBP
    /// </summary>
    public class AzureRateCard : IRateCardApi
    {
        /// <summary>
        /// Invoked using a subscription id - important to ensure that app service principal is at least in the read role for the "subscription"
        /// </summary>
        public AzureRateCard(IBillingRequest rateCardRequest, CustomerContainer customer)
        {
            Customer = customer;
            BillingRequest = rateCardRequest;
        }

        public AzureRateCard(CustomerContainer customer) : this(new RateCardBillingRequest(), customer) { }

        public IBillingRequest BillingRequest { get; set; }

        public AzureRateCard() { }
        /// <summary>
        /// Returns the subscription id pegged to the AzureRateCard
        /// </summary>
        public CustomerContainer Customer { get; set; }
        /// <summary>
        /// Returns a RateCard based on an of for the 
        /// </summary>
        /// <param name="offerId">e.g. MS-AZR-0121P - can leave it blank for PAYG</param>
        /// <returns>A serialized rate card containing all details for the rate card offer id</returns>
        public async Task<RateCard> GetRateCardForOffer(string offerId)
        {
            var payload = await GetRateCardForOfferAsString(offerId);
            return JsonConvert.DeserializeObject<RateCard>(payload);
        }

        private async Task<string> GetRateCardForOfferAsString(string offerId)
        {
            Customer.OfferId = Customer.OfferId ?? offerId;
            return await BillingRequest.MakeRequest(Customer);
        }
        /// <summary>
        /// Used to get a rate card that the API can't return - this can be supplied in a spreadsheet form 
        /// </summary>
        /// <param name="name">A lookup id for the database etc.</param>
        /// <returns>A rate card based on a name</returns>
        public RateCard GetProprietaryRateCard(string name)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Exports to either .csv or .json depending on the extension supplied
        /// </summary>
        /// <param name="offerId">The offer id used</param>
        /// <param name="fileName">The name of the file to export to</param>
        /// <returns>returns void</returns>
        public async Task Export(string offerId, string fileName)
        {
            var writer = new StreamWriter(fileName);
            var extension = Path.GetExtension(fileName);
            // if the extension is null or not a json or csv file then bale from this method
            if (extension == null || (extension.ToLower() != ".csv" && extension.ToLower() != ".json"))
            {
                throw new ApplicationException("Only able to save file data in .json or .csv");
            }
            // if JSON then dump out the contents in one go 
            if (extension == ".json")
            {
                await writer.WriteAsync(await GetRateCardForOfferAsString(offerId));
            }
            // if CSV then push this out to a file flattened by meter id as the key
            if (extension == ".csv")
            {
                var rateCard = await GetRateCardForOffer(offerId);
                string header = "SubscriptionId,MeterCategory,MeterId,MeterName,MeterRegion,MeterSubcategory,Price,Units,Currency";
                await writer.WriteLineAsync(header);
                // TODO: accident waiting to happen if for no reason there is no rate for a particular meter id
                foreach (string line in rateCard.Meters.Select(meter => $"{Customer.SubscriptionId},{meter.MeterCategory},{meter.MeterId},\"{meter.MeterName}\",{meter.MeterRegion},{meter.MeterSubCategory},{meter.MeterRates[0]},\"{meter.Unit}\",{rateCard.Currency}"))
                {
                    await writer.WriteLineAsync(line);
                }
            }
            writer.Close();
        }
    }
}
