using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    public class ConfigurationApiKeyStoreOptions
    {
        public IDictionary<string, string> Keys { get; set; }
    }
}
