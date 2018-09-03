using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Csg.AspNetCore.ApiKeyAuthentication
{
    public class ConfigurationApiKeyStore : IApiKeyStore
    {
        private Microsoft.Extensions.Options.IOptionsMonitor<ConfigurationApiKeyStoreOptions> _options;

        public ConfigurationApiKeyStore(Microsoft.Extensions.Options.IOptionsMonitor<ConfigurationApiKeyStoreOptions> options)
        {
            _options = options;
        }

        public Task<ApiKey> GetKeyAsync(string clientID)
        {
            if(_options.CurrentValue.Keys.TryGetValue(clientID, out string secret))
            {
                return Task.FromResult(new ApiKey() { ClientID = clientID, Secret = secret });
            }

            return Task.FromResult<ApiKey>(null);
        }

        public bool SupportsClaims { get { return false; } }

        public Task<ICollection<Claim>> GetClaimsAsync(ApiKey key)
        {
            throw new NotSupportedException();
        }
    }
}
