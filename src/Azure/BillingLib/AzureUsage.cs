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
using Newtonsoft.Json.Linq;

namespace BillingLib
{
    /// <summary>
    /// Used to get the usage details from Azure - this is pluggable and whilst the db is riged towards Azure should be able to support other providers
    /// </summary>
    public class AzureUsage : IUsageApi
    {
        public AzureUsage() { }
        public AzureUsage(IRateCardApi rateCard, IBillingRequest request, CustomerContainer customer)
        {
            Customer = customer;
            RateCard = rateCard;
            BillingRequest = request;
        }

        public IBillingRequest BillingRequest { get; set; }

        public IRateCardApi RateCard { get; set; }

        public AzureUsage(CustomerContainer customer) : this(new AzureRateCard(customer), new UsageBillingRequest(), customer)
        {
        }
        /// <summary>
        /// The subscription id used for AzureUsage 
        /// </summary>
        public CustomerContainer Customer { get; set; }

      /// <summary>
      /// Given a start date and end date gets all of the usage inbetween 
      /// </summary>
      public async Task<Usage> GetUsageByDate(DateTime startTime, DateTime endTime)
      {
         var usages = new List<UsageAggregate>();
         Customer.StartTime = startTime;
         Customer.EndTime = endTime;
         //var payload = await BillingRequest.MakeRequest(Customer);
         string nextLink = "dummy";
         while (!String.IsNullOrEmpty(nextLink))
         {
            var payload = await BillingRequest.MakeObjectRequest<NextPayload>(Customer);
            if (payload.Payload != null) {
               usages.AddRange(payload.Payload);
            }
            nextLink = Customer.RequestNextLink = payload.NextLink;
         }
         return new Usage
         {
            Value = usages,
            ReportedStartDate = startTime,
            ReportedEndData = endTime
         };
      }

        /// <summary>
        /// Given a start date and end date gets the usage and associated pricing 
        /// </summary>
        public async Task<Usage> GetCombinedRatesAndUsage(DateTime startTime, DateTime endTime, string offerId)
        {
            var rateCard = new AzureRateCard(Customer);
            var rates = await rateCard.GetRateCardForOffer(offerId);
            var usage = await GetUsageByDate(startTime, endTime);
            usage.ReportedStartDate = DateTime.Parse(startTime.ToString("yyyy-MM-ddTHH:00:00Z"));
            usage.ReportedEndData = DateTime.Parse(endTime.ToString("yyyy-MM-ddTHH:00:00Z"));
            // calculate the coincidence of usage and rates using meterid 
            foreach (var part in usage.Value)
            {
                var meter = rates.Meters.Find(meterInner => meterInner.MeterId == part.Properties.MeterId);
                // TODO: Need to do something here otherwise we'll lose information
                if (meter == null) continue;
                // set the rates data on the usage output
                var result = part.Properties.Quantity*meter.MeterRates[0];
                part.Properties.Cost = result;
                part.Properties.Price = meter.MeterRates[0];
                part.Properties.Currency = rates.Currency;
                part.Properties.ResourceGroup = GetResourceGroup(part);
                part.Properties.ResourceProvider = GetResourceProvider(part);
                part.Properties.ResourceName = GetResourceName(part);
                part.Properties.ResourceSubName = GetDetailedResourceName(part);
                part.Properties.Tags = part.Properties.InstanceDataRaw == null ? null : DeserializeTags(part.Properties.InstanceData?.MicrosoftResources?.Tags);
                part.Properties.Location = part.Properties.InstanceDataRaw == null ? null : part.Properties.InstanceData?.MicrosoftResources?.Location;
            }
            return usage;
        }

        /// <summary>
        /// Exports usage data to a .csv file 
        /// </summary>
        public async Task Export(string offerId, string fileName)
        {
            var writer = new StreamWriter(fileName);
            var extension = Path.GetExtension(fileName);
            // if the extension is null or not a json or csv file then bale from this method
            if (extension == null || extension.ToLower() != ".csv")
            {
                throw new ApplicationException("Only able to save file data in .csv");
            }
            // if CSV then push this out to a file flattened by meter id as the key
            if (extension == ".csv")
            {
                var rateCard = await GetLatestUsageAndRates(offerId);
                string header = "SubscriptionId,MeterCategory,MeterId,MeterName,MeterRegion,MeterSubcategory,Price,Cost,Currency,Units,StartTime,EndTime,ResourceGroup,ResourceProvider,ResourceName,Tags,Location";
                await writer.WriteLineAsync(header);
                // TODO: accident waiting to happen if for no reason there is no rate for a particular meter id
                foreach (var rateUsage in rateCard.Value)
                {
                    string resourceGroup = GetResourceGroup(rateUsage) ?? String.Empty;
                    string resourceProvider = GetResourceProvider(rateUsage) ?? String.Empty;
                    string resourceName = GetResourceName(rateUsage) ?? String.Empty;
                    string resourceSubName = GetDetailedResourceName(rateUsage) ?? String.Empty;
                    string tags = rateUsage.Properties.InstanceDataRaw == null ? String.Empty : (DeserializeTags(rateUsage.Properties.InstanceData?.MicrosoftResources?.Tags) ?? String.Empty);
                    string location = rateUsage.Properties.InstanceDataRaw == null ? String.Empty : (rateUsage.Properties.InstanceData?.MicrosoftResources?.Location ?? String.Empty);
                    await writer.WriteLineAsync($"{Customer.SubscriptionId},\"{rateUsage.Properties.MeterCategory}\",{rateUsage.Properties.MeterId},\"{rateUsage.Properties.MeterName}\",{rateUsage.Properties.MeterRegion},{rateUsage.Properties.MeterSubCategory},{rateUsage.Properties.Price},{rateUsage.Properties.Cost},{rateUsage.Properties.Currency},\"{rateUsage.Properties.Unit}\",{rateUsage.Properties.UsageStartTime},{rateUsage.Properties.UsageEndTime},{resourceGroup},{resourceProvider},{resourceName},{resourceSubName},{tags},{location}");
                }
            }
            writer.Close();
        }

        /// <summary>
        /// Gets the latest rates on offer and combines them with the usage for the previous hour
        /// </summary>
        public Task<Usage> GetLatestUsageAndRates(string offerId)
        {
            return GetCombinedRatesAndUsage(DateTime.UtcNow.Subtract(TimeSpan.FromHours(3)),
                DateTime.UtcNow.Subtract(TimeSpan.FromHours(2)), offerId);
        }

        #region Resource Group gets

        private string GetResourceGroup(UsageAggregate rateUsage)
        {
            return GetDetailFromResourceUriString(rateUsage, "resourceGroups");
        }

        private string GetResourceProvider(UsageAggregate rateUsage)
        {
            return GetDetailFromResourceUriString(rateUsage, "providers");
        }

        private string GetResourceName(UsageAggregate rateUsage)
        {
            return GetDetailFromResourceUriStringByOrdinal(rateUsage);
        }

        private string GetDetailedResourceName(UsageAggregate rateUsage)
        {
            return GetLastPositionFromResourceUriString(rateUsage);
        }

        private string GetDetailFromResourceUriString(UsageAggregate rateUsage, string stringtoMatch)
        {
            if (rateUsage.Properties.InstanceDataRaw != null && rateUsage.Properties.InstanceData?.MicrosoftResources?.ResourceUri != null)
            {
                var instanceParts = rateUsage.Properties.InstanceData.MicrosoftResources.ResourceUri.Split('/');
                for (int i = 0; i < instanceParts.Length; i++)
                {
                    if (instanceParts[i] == stringtoMatch)
                    {
                        return instanceParts[i + 1];
                    }
                }
            }
            else if (rateUsage.Properties.InfoFields?.Project != null && stringtoMatch == "resourceGroups")
            {
               return rateUsage.Properties.InfoFields.Project;
            }

            return null;
        }

        private string GetDetailFromResourceUriStringByOrdinal(UsageAggregate rateUsage, int ordinal = 8)
        {
            if (rateUsage.Properties.InstanceDataRaw != null && rateUsage.Properties.InstanceData?.MicrosoftResources?.ResourceUri != null)
            {
                try
                {
                    var instanceParts = rateUsage.Properties.InstanceData.MicrosoftResources.ResourceUri.Split('/');
                    if (instanceParts.Length > 7)
                        return instanceParts[8];
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return "";
        }

        private string GetLastPositionFromResourceUriString(UsageAggregate rateUsage)
        {
            if (rateUsage.Properties.InstanceDataRaw != null && rateUsage.Properties.InstanceData?.MicrosoftResources?.ResourceUri != null)
            {
                try
                {
                    var instanceParts = rateUsage.Properties.InstanceData.MicrosoftResources.ResourceUri.Split('/');
                    return instanceParts[instanceParts.Length - 1];
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        #endregion


        #region Tags gets 

        private string DeserializeTags(IDictionary<string, string> tags)
        {
            string output = tags?.Aggregate<KeyValuePair<string, string>, string>(null, (current, tag) => current + $"{tag.Key}:{tag.Value};");
            return output?.Length > 0 ? output.Substring(0, output.Length - 1) : null;
        } 

        #endregion 
    }
}