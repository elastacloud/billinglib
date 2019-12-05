namespace AadHelper
{
    /// <summary>
    /// The subscription that is part of the AAD
    /// </summary>
    public class Subscription
    {
        /// <summary>
        /// The subscription name 
        /// </summary>
        public string DisplayName { get; set; }
        /// <summary>
        /// The subscription id (GUID)
        /// </summary>
        public string SubscriptionId { get; set; }
    }
}