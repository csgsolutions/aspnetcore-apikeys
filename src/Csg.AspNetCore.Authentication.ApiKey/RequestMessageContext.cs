using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    public class RequestMessageContext : Microsoft.AspNetCore.Authentication.ResultContext<ApiKeyOptions>
    {
        public RequestMessageContext(HttpContext context, AuthenticationScheme scheme, ApiKeyOptions options) : base(context, scheme, options)
        {
        }

        public string AuthenticationType { get; set; }

        public string ClientID { get; set; }

        public string Token { get; set; }
    }
}
