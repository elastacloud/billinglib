using System.Collections.Generic;
using Newtonsoft.Json;

namespace BillingLib
{
    public class MicrosoftResourcesDataType
    {
        [JsonProperty("resourceUri")]
        public string ResourceUri { get; set; }

        [JsonProperty("tags")]
        public IDictionary<string, string> Tags { get; set; }
        [JsonProperty("additionalInfo")]
        public IDictionary<string, string> AdditionalInfo { get; set; }
        [JsonProperty("location")]
        public string Location { get; set; }
        [JsonProperty("partNumber")]
        public string PartNumber { get; set; }
        [JsonProperty("orderNumber")]
        public string OrderNumber { get; set; }
    }
}