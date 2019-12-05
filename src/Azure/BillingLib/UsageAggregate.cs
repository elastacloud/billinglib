using Newtonsoft.Json;

namespace BillingLib
{
    public class UsageAggregate
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("properties")]
        public Properties Properties { get; set; }
    }
}