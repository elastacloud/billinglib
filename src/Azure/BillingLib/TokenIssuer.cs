using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AadHelper;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace BillingLib
{
    public class TokenIssuer
    {
        public async Task<string> GetOAuthTokenFromAad(CustomerContainer customer)
        {
            var authenticationContext = new AuthenticationContext(
                $"{customer.AdalServiceUrl}/{customer.TenantDomain}");

            //Ask the logged in user to authenticate, so that this client app can get a token on his behalf
            var result = await authenticationContext.AcquireTokenAsync(
                $"{customer.ArmBillingServiceUrl}/",
                new ClientCredential(customer.ClientId, customer.ClientKey));

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            return result.AccessToken;
        }
    }
}
