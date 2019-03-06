using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    public class TimeBasedApiKeyValidator : IApiKeyValidator
    {
        private readonly Csg.ApiKeyGenerator.TimeBasedTokenGenerator _generator;
        private readonly Microsoft.AspNetCore.Authentication.ISystemClock _clock;

        public TimeBasedApiKeyValidator(Microsoft.AspNetCore.Authentication.ISystemClock clock)
        {
            _generator = new Csg.ApiKeyGenerator.TimeBasedTokenGenerator();
            _clock = clock;
        }

        public TimeBasedApiKeyValidator(Microsoft.AspNetCore.Authentication.ISystemClock clock, Csg.ApiKeyGenerator.TimeBasedTokenGenerator generator)
        {
            _generator = generator;
            _clock = clock;
        }

        public Task<bool> ValidateKeyAsync(ApiKey keyFromStore, string token)
        {
            var now = _clock.UtcNow;

            var tokenBytes = Microsoft.AspNetCore.WebUtilities.Base64UrlTextEncoder.Decode(token);

            if (!_generator.ValidateToken(keyFromStore.ClientID, keyFromStore.Secret, tokenBytes, now))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
