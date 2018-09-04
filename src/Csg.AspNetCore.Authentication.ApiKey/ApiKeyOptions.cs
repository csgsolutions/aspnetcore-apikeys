using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    public class ApiKeyOptions : Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions
    {
        public string HeaderName { get; set; } = "Authorization";

        public string AuthenticationType { get; set; } = "ApiKey";

        public new ApiKeyEvents Events
        {
            get
            {
                return (ApiKeyEvents)base.Events;
            }
            set
            {
                base.Events = value;
            }
        }

        public IApiKeyValidator KeyValidator { get; set; }
        
        public bool StaticKeyEnabled { get; set; } = true;

        public bool TimeBasedKeyEnabled { get; set; } = true;

        public int TimeBasedKeyInterval { get; set; } = 60;

        public int TimeBasedKeyTolerance { get; set; } = 2;
        
        public ApiKeyOptions() : base()
        {
            this.Events = new ApiKeyEvents();
        }
    }
}
