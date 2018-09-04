using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    public class ApiKey
    {
        public string ClientID { get; set; }
        public string Secret { get; set; }
    }
}
