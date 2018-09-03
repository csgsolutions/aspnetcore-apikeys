using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Csg.AspNetCore.ApiKeyAuthentication
{
    public interface IApiKeyStore
    {
        Task<ApiKey> GetKeyAsync(string keyName);

        bool SupportsClaims { get; }

        Task<System.Collections.Generic.ICollection<System.Security.Claims.Claim>> GetClaimsAsync(ApiKey key);
    }
}
