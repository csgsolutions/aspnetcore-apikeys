using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    public class TimeBasedApiKeyValidator : IApiKeyValidator
    {
        private readonly Csg.AspNetCore.Authentication.ApiKey.TimeBasedTokenGenerator _generator;
        private readonly Microsoft.AspNetCore.Authentication.ISystemClock _clock;

        public TimeBasedApiKeyValidator(Microsoft.AspNetCore.Authentication.ISystemClock clock)
        {
            _generator = new Csg.AspNetCore.Authentication.ApiKey.TimeBasedTokenGenerator();
            _clock = clock;
        }

        public TimeBasedApiKeyValidator(Microsoft.AspNetCore.Authentication.ISystemClock clock, Csg.AspNetCore.Authentication.ApiKey.TimeBasedTokenGenerator generator)
        {
            _generator = generator;
            _clock = clock;
        }

        public Task<bool> ValidateKeyAsync(ApiKey keyFromStore, string token)
        {
            var now = _clock.UtcNow;

            if (!_generator.ValidateToken(keyFromStore.ClientID, keyFromStore.Secret, token, now))
            {
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
