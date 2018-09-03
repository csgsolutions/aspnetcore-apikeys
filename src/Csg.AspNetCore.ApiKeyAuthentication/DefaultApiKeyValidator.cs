using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Csg.AspNetCore.ApiKeyAuthentication
{
    public class DefaultApiKeyValidator : IApiKeyValidator
    {
        public Task<bool> ValidateKeyAsync(ApiKey keyFromStore, ApiKey providedKey)
        {
            if (keyFromStore == null) throw new ArgumentNullException(nameof(keyFromStore));
            if (providedKey == null) throw new ArgumentNullException(nameof(providedKey));

            //TODO: should do a slow compare
            return Task.FromResult<bool>(keyFromStore.Secret.Equals(providedKey.Secret));
        }
    }
}
