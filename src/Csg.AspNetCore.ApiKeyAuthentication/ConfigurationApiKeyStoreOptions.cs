using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.ApiKeyAuthentication
{
    public class ConfigurationApiKeyStoreOptions
    {
        public IDictionary<string, string> Keys { get; set; }
    }
}
