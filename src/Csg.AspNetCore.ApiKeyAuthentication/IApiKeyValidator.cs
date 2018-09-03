using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.ApiKeyAuthentication
{
    public interface IApiKeyValidator
    {
        System.Threading.Tasks.Task<bool> ValidateKeyAsync(ApiKey keyFromStore, ApiKey providedKey);
    }
}
