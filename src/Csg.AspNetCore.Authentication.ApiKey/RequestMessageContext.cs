using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    public class RequestMessageContext
    {
        public AuthenticateResult Result { get; set; }

        public string AuthenticationType { get; set; }

        public string ClientID { get; set; }

        public string Token { get; set; }
    }
}
