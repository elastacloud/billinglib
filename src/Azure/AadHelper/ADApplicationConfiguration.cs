using System.Collections.Generic;

namespace AadHelper
{
    public class ADApplicationConfiguration
    {
        public string DisplayName { get; set; }
        public List<string> ReplyUrls { get; set; }
        public string TenantId { get; set; }
        public string SubscriptionId { get; set; }
    }
}