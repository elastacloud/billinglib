using Newtonsoft.Json;

namespace BillingLib
{
    public class InfoFields
    {
        [JsonProperty("resourceUri")]
        public string ResourceUri { get; set; }
        [JsonProperty("meteredService")]
        public string MeteredService { get; set; }
        [JsonProperty("project")]
        public string Project { get; set; }
        [JsonProperty("meteredServiceType")]
        public string MeteredServiceType { get; set; }
        [JsonProperty("serviceInfo1")]
        public string ServiceInfo1 { get; set; } 
    }
}