using AadHelper;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingLib
{
    /// <summary>
    /// Used to return the usage data for the subscription 
    /// </summary>
    public interface IUsageApi
    {
        /// <summary>
        /// Given date boundaries returns the usage for this period
        /// </summary>
        Task<Usage> GetUsageByDate(DateTime startTime, DateTime endTime);
        /// <summary>
        /// Given the data boundaries and rate card returns priced usage for this period
        /// </summary>
        Task<Usage> GetCombinedRatesAndUsage(DateTime startTime, DateTime endTime, string offerId);
        /// <summary>
        /// Exports the data in .csv or .json
        /// </summary>
        Task Export(string offerId, string fileName);
        /// <summary>
        /// Gets the latest usage and rates data
        /// </summary>
        Task<Usage> GetLatestUsageAndRates(string offerId);

        CustomerContainer Customer { set; get; }
        IRateCardApi RateCard { get; set; }
    }
}
