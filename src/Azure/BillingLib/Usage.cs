using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BillingLib
{
    public class Usage
    {
        [JsonProperty("value")]
        public List<UsageAggregate> Value { get; set; } 
        public DateTime ReportedStartDate { get; set; }
        public DateTime ReportedEndData { get; set; }
        [JsonProperty("NextLink")]
        public string NextLink { get; set; }
    }



    public class StartEndTime
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}