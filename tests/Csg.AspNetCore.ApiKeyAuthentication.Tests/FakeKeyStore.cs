using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Csg.AspNetCore.ApiKeyAuthentication.Tests
{
    public class FakeKeyStore : Csg.AspNetCore.ApiKeyAuthentication.IApiKeyStore
    {
        public bool SupportsClaims => false;

        public Task<ICollection<Claim>> GetClaimsAsync(ApiKey key)
        {
            throw new NotImplementedException();
        }

        public Task<ApiKey> GetKeyAsync(string keyName)
        {
            if (keyName == "TestName")
            {
                return Task.FromResult(new ApiKey() { ClientID = "TestName", Secret = "TestKey" });
            }
            else
            {
                return Task.FromResult<ApiKey>(null);
            }
        }
    }
}
