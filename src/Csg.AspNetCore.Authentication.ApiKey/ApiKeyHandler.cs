using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace Csg.AspNetCore.Authentication.ApiKey
{
    public class ApiKeyHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<ApiKeyOptions>
    {
        private const string AuthorizationHeader = "Authorization";
        private const string AuthTypeBasic = "Basic";
        private const string AuthTypeApiKey = "ApiKey";
        private const string AuthTypeTApiKey = "TApiKey";

        private const string InvalidAuthHeaderMessage = "Invalid authorization header";
        private const string InvalidApiKeyMessage = "Invalid API Key";
        private const string InvalidClientMessage = "Invalid ClientID";

        private readonly IApiKeyStore _keyStore;
        
        public ApiKeyHandler(IApiKeyStore keyStore, IOptionsMonitor<ApiKeyOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
        {
            _keyStore = keyStore;
        }
       
        public void GetApiKeyFromRequest(RequestMessageContext messageContext)
        {
            if (!this.Request.Headers.TryGetValue(AuthorizationHeader, out StringValues headerValue))
            {
                messageContext.NoResult();
                return;
            }

            var rawHeader = headerValue[0].AsSpan();
            var indexOfFirstSpace = rawHeader.IndexOf(' ');

            if (indexOfFirstSpace <= 0)
            {
                messageContext.NoResult();
                return;
            }

            var SAuthTypeBasic = AuthTypeBasic.AsSpan();
            var SAuthTypeApiKey = AuthTypeApiKey.AsSpan();
            var SAuthTypeTApiKey = AuthTypeTApiKey.AsSpan();
            var authType = rawHeader.Slice(0, indexOfFirstSpace);

            if (this.Options.HttpBasicEnabled && authType.Equals(SAuthTypeBasic, StringComparison.OrdinalIgnoreCase))
            {
                this.Logger.LogDebug($"HTTP Basic authentication detected.");

                var valueEncoded = rawHeader.Slice(indexOfFirstSpace + 1);
                var valueDecoded = System.Text.UTF8Encoding.UTF8.GetString(Convert.FromBase64CharArray(valueEncoded.ToArray(), 0, valueEncoded.Length)).AsSpan();
                var split = valueDecoded.IndexOf(':');

                messageContext.ClientID = valueDecoded.Slice(0, split).ToString();
                messageContext.Token = valueDecoded.Slice(split + 1).ToString();
                messageContext.AuthenticationType = AuthTypeBasic;

                return;
            }
                       
            if ((this.Options.TimeBasedKeyEnabled && authType.Equals(SAuthTypeApiKey, StringComparison.OrdinalIgnoreCase))
                || (this.Options.StaticKeyEnabled && authType.Equals(SAuthTypeTApiKey, StringComparison.OrdinalIgnoreCase)))
            {
                this.Logger.LogDebug($"Authorization {authType.ToString()} detected.");

                var indexOfFirstColon = rawHeader.IndexOf(':');

                if (indexOfFirstColon <= 0)
                {
                    messageContext.Fail(InvalidAuthHeaderMessage);
                    return;
                }

                messageContext.ClientID = rawHeader.Slice(indexOfFirstSpace + 1, indexOfFirstColon - indexOfFirstSpace - 1).ToString();
                messageContext.Token = rawHeader.Slice(indexOfFirstColon + 1).ToString();
                messageContext.AuthenticationType = authType.ToString();

                return;
            }

            messageContext.NoResult();
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var requestMessage = new RequestMessageContext(this.Context, this.Scheme, this.Options);

            await this.Options.Events.OnRequestAsync(requestMessage);

            if (requestMessage.Result != null)
            {
                this.Logger.LogDebug("Using the result returned from the OnRequestAsync event.");
                return requestMessage.Result;
            }

            // if the token wasn't set in OnRequestAsync(), then try to get it from the Authorization or custom header
            if (requestMessage.Token == null)
            {
                GetApiKeyFromRequest(requestMessage);
            }

            if (requestMessage.Result != null)
            {
                return requestMessage.Result;
            }

            if (requestMessage?.ClientID == null)
            {
                this.Logger.LogDebug("ClientID not provided or is malformed.");
                return AuthenticateResult.Fail(InvalidClientMessage);
            }

            var keyFromStore = await _keyStore.GetKeyAsync(requestMessage.ClientID);

            if (keyFromStore == null)
            {
                this.Logger.LogInformation("An API key could not be found for the given ClientID.");
                return AuthenticateResult.Fail(InvalidClientMessage);
            }

            var keyValidator = this.Options.KeyValidator;

            if (requestMessage.AuthenticationType.Equals(AuthTypeBasic, StringComparison.OrdinalIgnoreCase))
            {
                keyValidator = keyValidator ?? new DefaultApiKeyValidator();
            }
            else if (requestMessage.AuthenticationType.Equals(AuthTypeApiKey, StringComparison.OrdinalIgnoreCase))
            {
                keyValidator = keyValidator ?? new DefaultApiKeyValidator();
            }
            else if (requestMessage.AuthenticationType.Equals(AuthTypeTApiKey, StringComparison.OrdinalIgnoreCase))
            {
                keyValidator = keyValidator ?? new TimeBasedApiKeyValidator(this.Clock, new Csg.ApiKeyGenerator.TimeBasedTokenGenerator()
                {
                    IntervalSeconds = this.Options.TimeBasedKeyInterval,
                    AllowedNumberOfDriftIntervals = this.Options.TimeBasedKeyTolerance
                });
            }
            else if (keyValidator == null)
            {
                return AuthenticateResult.NoResult();
            }

            if (!(await keyValidator.ValidateKeyAsync(keyFromStore, requestMessage.Token)))
            {
                this.Logger.LogInformation($"The ClientID and Key pair provided in the request ({requestMessage.ClientID}, {requestMessage.Token}) is not valid.");
                return AuthenticateResult.Fail(InvalidApiKeyMessage);
            }

            var userResult = await CreateIdentityAsync(keyFromStore);
            
            if (userResult.Result != null)
            {
                return userResult.Result;
            }
            
            return AuthenticateResult.Success(new AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(userResult.Identity), this.Scheme.Name));
        }

        private async Task<AuthenticatedEventContext> CreateIdentityAsync(ApiKey key)
        {
            var claims = new List<System.Security.Claims.Claim>();

            if (_keyStore.SupportsClaims)
            {
                claims.AddRange(await _keyStore.GetClaimsAsync(key));
            }
            else
            {
                claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, key.ClientID));
            }

            var identity = new System.Security.Claims.ClaimsIdentity(claims, this.Options.AuthenticationType);
            var eventContext = new AuthenticatedEventContext(this.Context, this.Scheme, this.Options, key.ClientID, identity);

            await this.Options.Events.OnAuthenticatedAsync(eventContext);

            return eventContext;
        }
    }
}
