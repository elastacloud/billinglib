using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingLib
{
    //TODO: Add a test to make sure start and end times are valid?
    public class QueueMessage
    {
        public string Type { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
    }
}
