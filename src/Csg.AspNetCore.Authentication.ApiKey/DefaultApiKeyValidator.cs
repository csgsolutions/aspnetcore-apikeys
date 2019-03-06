using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    public class DefaultApiKeyValidator : IApiKeyValidator
    {
        public Task<bool> ValidateKeyAsync(ApiKey keyFromStore, string token)
        {
            if (keyFromStore == null) throw new ArgumentNullException(nameof(keyFromStore));
            if (token == null) throw new ArgumentNullException(nameof(token));

            //TODO: should do a slow compare
            return Task.FromResult<bool>(keyFromStore.Secret.Equals(token));
        }
    }
}
