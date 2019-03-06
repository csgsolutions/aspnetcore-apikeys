using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    public interface IApiKeyStore
    {
        Task<ApiKey> GetKeyAsync(string clientID);

        bool SupportsClaims { get; }

        Task<System.Collections.Generic.ICollection<System.Security.Claims.Claim>> GetClaimsAsync(ApiKey key);
    }
}
