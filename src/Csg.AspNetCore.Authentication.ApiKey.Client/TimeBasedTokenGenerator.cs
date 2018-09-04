using System;
using System.Collections.Generic;
using System.Text;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    public class TimeBasedTokenGenerator
    {
        public System.Security.Cryptography.HashAlgorithmName HashMethod { get; set; } = System.Security.Cryptography.HashAlgorithmName.SHA256;

        public int IntervalSeconds { get; set; } = 60;

        public int NumberOfIntervalsOfTolerance { get; set; } = 2;

        public string GenerateToken(string clientID, string secret, DateTimeOffset utcNow)
        {
            var hmac = new TimeBasedHmac(string.Concat("HMAC",this.HashMethod.Name));

            hmac.IntervalSeconds = this.IntervalSeconds;

            var hash = hmac.ComputeAuthenticator(UTF8Encoding.UTF8.GetBytes(clientID), secret, utcNow.DateTime);

            return Convert.ToBase64String(hash);
        }

        public bool ValidateToken(string clientID, string secret, string token, DateTimeOffset utcNow)
        {
            var hmac = new TimeBasedHmac(string.Concat("HMAC", this.HashMethod.Name));

            hmac.AllowDriftSteps = this.NumberOfIntervalsOfTolerance;
            hmac.IntervalSeconds = this.IntervalSeconds;

            return hmac.ValidateAuthenticator(UTF8Encoding.UTF8.GetBytes(clientID), Convert.FromBase64String(token), secret, utcNow.DateTime);
        }
    }
}
