using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.ApiKeyAuthentication
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

        public IApiKeyValidator KeyValidator { get; set; } = new DefaultApiKeyValidator();

        public ApiKeyOptions() : base()
        {
            this.Events = new ApiKeyEvents();
        }
    }
}
