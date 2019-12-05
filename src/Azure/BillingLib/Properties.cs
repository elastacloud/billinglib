using System.Collections.Generic;
using Newtonsoft.Json;

namespace BillingLib
{
    public class Properties
    {
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }
        [JsonProperty("usageStartTime")]
        public string UsageStartTime { get; set; }
        [JsonProperty("usageEndTime")]
        public string UsageEndTime { get; set; }
        [JsonProperty("meterId")]
        public string MeterId { get; set; }
        [JsonProperty("infoFields")]
        public InfoFields InfoFields { get; set; }

        [JsonProperty("instanceData")]
        public string InstanceDataRaw { get; set; }

        public InstanceDataType InstanceData => JsonConvert.DeserializeObject<InstanceDataType>(InstanceDataRaw.Replace("\\\"", ""));
        [JsonProperty("quantity")]
        public double Quantity { get; set; }
        [JsonProperty("unit")]
        public string Unit { get; set; }
        [JsonProperty("meterName")]
        public string MeterName { get; set; }
        [JsonProperty("meterCategory")]
        public string MeterCategory { get; set; }
        [JsonProperty("meterSubCategory")]
        public string MeterSubCategory { get; set; }
        [JsonProperty("meterRegion")]
        public string MeterRegion { get; set; }
        public double Cost { get; set; }
        public double Price { get; set; }
        public string Currency { get; set; }
        public string ResourceGroup { get; set; }
        public string ResourceProvider { get; set; }
        public string ResourceName { get; set; }
        public string ResourceSubName { get; set; }
        public string Tags { get; set; }
        public string Location { get; set; }
    }
}