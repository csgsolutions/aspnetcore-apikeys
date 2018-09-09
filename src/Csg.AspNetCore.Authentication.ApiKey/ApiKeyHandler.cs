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
            var indexOfFirstColon = rawHeader.IndexOf(':');

            if (indexOfFirstSpace <= 0)
            {
                messageContext.NoResult();
                return;
            }

            string authType = rawHeader.Slice(0, indexOfFirstSpace).ToString();

            if (!(authType.Equals("ApiKey", StringComparison.OrdinalIgnoreCase) || authType.Equals("TApiKey", StringComparison.OrdinalIgnoreCase)))
            {
                messageContext.NoResult();
                return;
            }

            if (indexOfFirstColon <= 0)
            {
                messageContext.Fail("Invalid authorization header");
                return;
            }

            messageContext.ClientID = rawHeader.Slice(indexOfFirstSpace + 1, indexOfFirstColon-indexOfFirstSpace-1).ToString();
            messageContext.Token = rawHeader.Slice(indexOfFirstColon+1).ToString();
            messageContext.AuthenticationType = authType;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var requestMessage = new RequestMessageContext(this.Context, this.Scheme, this.Options);

            await this.Options.Events.OnRequestAsync(requestMessage);

            if (requestMessage.Result != null)
            {
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
                return AuthenticateResult.Fail("Client is not valid.");
            }

            var keyFromStore = await _keyStore.GetKeyAsync(requestMessage.ClientID);

            if (keyFromStore == null)
            {
                this.Logger.LogInformation("An API key could not be found for the given ClientID.");
                return AuthenticateResult.Fail("Invalid ClientID");
            }

            var keyValidator = this.Options.KeyValidator;

            if (requestMessage.AuthenticationType.Equals("APIKEY", StringComparison.OrdinalIgnoreCase))
            {
                if (!this.Options.StaticKeyEnabled)
                {
                    this.Logger.LogInformation($"The client specified an authentication type '{requestMessage.AuthenticationType}' that is not enabled.");
                    return AuthenticateResult.Fail("Invalid authentication type");
                }

                keyValidator = keyValidator ?? new DefaultApiKeyValidator();
            }
            else if (requestMessage.AuthenticationType.Equals("TAPIKEY", StringComparison.OrdinalIgnoreCase))
            {
                if (!this.Options.TimeBasedKeyEnabled)
                {
                    this.Logger.LogInformation($"The client specified an authentication type '{requestMessage.AuthenticationType}' that is not enabled.");
                    return AuthenticateResult.Fail("Invalid authentication type");
                }

                keyValidator = keyValidator ?? new TimeBasedApiKeyValidator(this.Clock, new Csg.ApiKeyGenerator.TimeBasedTokenGenerator()
                {
                    IntervalSeconds = this.Options.TimeBasedKeyInterval,
                });
            }
            else if (keyValidator == null)
            {
                return AuthenticateResult.NoResult();
            }

            if (!(await keyValidator.ValidateKeyAsync(keyFromStore, requestMessage.Token)))
            {
                this.Logger.LogInformation("The provided API key is not valid.");
                return AuthenticateResult.Fail("Invalid API Key");
            }

            var userResult = await CreateUserAsync(keyFromStore);
            
            if (userResult.Result != null)
            {
                return userResult.Result;
            }
            
            return AuthenticateResult.Success(new AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(userResult.Identity), this.Scheme.Name));
        }

        private async Task<AuthenticatedEventContext> CreateUserAsync(ApiKey key)
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
