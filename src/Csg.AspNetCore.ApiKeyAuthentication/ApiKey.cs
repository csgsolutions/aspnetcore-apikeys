using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.ApiKeyAuthentication
{
    public class ApiKey
    {
        public string ClientID { get; set; }
        public string Secret { get; set; }
    }
}
