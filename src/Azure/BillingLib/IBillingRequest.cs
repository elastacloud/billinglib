using AadHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BillingLib
{
    public interface IBillingRequest
    {
        Task<string> MakeRequest(CustomerContainer container);
        Task<T> MakeObjectRequest<T>(CustomerContainer container);
    }
}
