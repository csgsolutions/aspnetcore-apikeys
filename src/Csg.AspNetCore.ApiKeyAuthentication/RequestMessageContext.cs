using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.ApiKeyAuthentication
{
    public class RequestMessageContext
    {
        public AuthenticateResult Result { get; set; }

        public ApiKey Key { get; set; }
    }
}
