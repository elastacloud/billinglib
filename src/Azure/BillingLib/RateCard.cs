using System;
using System.Collections.Generic;

namespace BillingLib
{
    public class RateCard
    {

        public List<object> OfferTerms { get; set; }
        public List<Resource> Meters { get; set; }
        public string Currency { get; set; }
        public string Locale { get; set; }
        public string RatingDate { get; set; }
        public bool IsTaxIncluded { get; set; }

    }
}