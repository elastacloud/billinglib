namespace AadHelper
{
    /// <summary>
    /// The subscription and resource group
    /// </summary>
    public class SubscriptionResourceGroup
    {
        /// <summary>
        /// The guid name of the subscription 
        /// </summary>
        public Subscription Subscription { get; set; }
        /// <summary>
        /// The resource group in the subscription
        /// </summary>
        public string ResourceGroup { get; set; }
    }
}