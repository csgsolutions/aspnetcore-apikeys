using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    public struct AuthorizationHeaderValue
    {
        public string AuthenticationType { get; private set; }

        public string Value { get; private set; }

        public static bool TryParse(Microsoft.Extensions.Primitives.StringValues authorizationHeaderValue, out AuthorizationHeaderValue values)
        {
            if (authorizationHeaderValue.Count != 1)
            {
                throw new ArgumentException("Header value must contain one and only one value.", nameof(authorizationHeaderValue));
            }

            values = new AuthorizationHeaderValue();

            var rawValue = authorizationHeaderValue[0].AsSpan();
            var indexOfFirstSpace = rawValue.IndexOf(' ');

            if (indexOfFirstSpace <= 0)
            {
                return false;
            }

            values.AuthenticationType = rawValue.Slice(0, indexOfFirstSpace).ToString();
            values.Value = rawValue.Slice(indexOfFirstSpace + 1).ToString();

            return true;
        }
    }
}
