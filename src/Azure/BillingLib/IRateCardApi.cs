using AadHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingLib
{
    public interface IRateCardApi
    {
        Task<RateCard> GetRateCardForOffer(string offerId);
        RateCard GetProprietaryRateCard(string name);
        Task Export(string offerId, string fileName);
        CustomerContainer Customer { set; get; }
    }
}
