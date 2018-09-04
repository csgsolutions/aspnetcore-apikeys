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

        //public RequestMessageContext GetApiKeyFromHeader()
        //{
        //    this.Logger.LogDebug($"Attempting to retrieve API key value from header ${this.Options.HeaderName}.");
        //    var message = new RequestMessageContext();

        //    if (!this.Request.Headers.TryGetValue(this.Options.HeaderName, out Microsoft.Extensions.Primitives.StringValues headerValue))
        //    {
        //        return message;
        //    }

        //    var parts = headerValue[0].Split(':');

        //    if (parts.Length != 2)
        //    {
        //        message.Result = AuthenticateResult.Fail("Invalid API Key format.");
        //        return message;
        //    }
            
        //    message.Key = new ApiKey()
        //    {
        //        ClientID = parts[0],
        //        Secret = parts[1]
        //    };

        //    return message;
        //}

        //public RequestMessageContext GetApiKeyFromAuthorizationHeader()
        //{
        //    this.Logger.LogDebug($"Attempting to retrieve API key value from Authorization header.");

        //    var message = new RequestMessageContext();

        //    if (!this.Request.Headers.TryGetValue(AuthorizationHeader, out Microsoft.Extensions.Primitives.StringValues headerValue))
        //    {
        //        message.Result = AuthenticateResult.NoResult();
        //        return message;
        //    }

        //    if (!AuthorizationHeaderValue.TryParse(headerValue, out AuthorizationHeaderValue auth_header))
        //    {
        //        message.Result = AuthenticateResult.NoResult();
        //        return message;
        //    }

        //    if (!auth_header.AuthenticationType.Equals(this.Options.AuthenticationType, StringComparison.OrdinalIgnoreCase))
        //    {
        //        message.Result = AuthenticateResult.NoResult();
        //        return message;
        //    }
            
        //    string[] valueParts = auth_header.Value.Split(':');

        //    if (valueParts.Length != 2)
        //    {
        //        message.Result = AuthenticateResult.Fail("Invalid API Key format.");
        //        return message;
        //    }

        //    message.Key = new ApiKey()
        //    {
        //        ClientID = valueParts[0],
        //        Secret = valueParts[1]
        //    };

        //    return message;
        //}

        public RequestMessageContext GetApiKeyFromRequest()
        {
            if (!this.Request.Headers.TryGetValue(AuthorizationHeader, out StringValues headerValue))
            {
                return new RequestMessageContext() { Result = AuthenticateResult.NoResult() };
            }

            var rawHeader = headerValue[0].AsSpan();
            var indexOfFirstSpace = rawHeader.IndexOf(' ');
            var indexOfFirstColon = rawHeader.IndexOf(':');

            if (indexOfFirstSpace <= 0)
            {
                return new RequestMessageContext() { Result = AuthenticateResult.NoResult() };
            }

            string authType = rawHeader.Slice(0, indexOfFirstSpace).ToString();

            if (!(authType.Equals("ApiKey", StringComparison.OrdinalIgnoreCase) || authType.Equals("TApiKey", StringComparison.OrdinalIgnoreCase)))
            {
                return new RequestMessageContext() { Result = AuthenticateResult.NoResult() };
            }

            if (indexOfFirstColon <= 0)
            {
                return new RequestMessageContext() { Result = AuthenticateResult.Fail("Malformed authorization header") };
            }

            string clientID = rawHeader.Slice(indexOfFirstSpace + 1, indexOfFirstColon-indexOfFirstSpace-1).ToString();
            string secret = rawHeader.Slice(indexOfFirstColon+1).ToString();

            return new RequestMessageContext()
            {
                ClientID = clientID,
                Token = System.Net.WebUtility.UrlDecode(secret),
                AuthenticationType = authType
            };
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var requestMessage = new RequestMessageContext();

            await this.Options.Events.OnRequestAsync(requestMessage);

            if (requestMessage.Result != null)
            {
                return requestMessage.Result;
            }

            // if the key wasn't set in OnRequestAsync(), then try to get it from the Authorization or custom header
            if (requestMessage.Token == null)
            {
                requestMessage = GetApiKeyFromRequest();
                //// Get clientid and key from the authorization header or another header
                //if (this.Options.HeaderName.Equals("Authorization", StringComparison.OrdinalIgnoreCase))
                //{
                //    requestMessage = GetApiKeyFromAuthorizationHeader();
                //}
                //else
                //{
                //    requestMessage = GetApiKeyFromHeader();
                //}
            }

            if (requestMessage.Result != null)
            {
                return requestMessage.Result;
            }

            if (requestMessage?.ClientID == null)
            {
                this.Logger.LogInformation("ClientID not provided or is malformed.");

                return AuthenticateResult.Fail("Client is not valid.");
            }

            var keyFromStore = await _keyStore.GetKeyAsync(requestMessage.ClientID);

            if (keyFromStore == null)
            {
                this.Logger.LogInformation("Failed to locate the API key.");

                return AuthenticateResult.Fail("Client is not valid.");
            }

            var keyValidator = this.Options.KeyValidator;

            if (requestMessage.AuthenticationType.Equals("ApiKey", StringComparison.OrdinalIgnoreCase))
            {
                if (!this.Options.StaticKeyEnabled)
                {
                    return AuthenticateResult.Fail("Authentication type not supported.");
                }

                keyValidator = keyValidator ?? new DefaultApiKeyValidator();
            }
            else if (requestMessage.AuthenticationType.Equals("TApiKey", StringComparison.OrdinalIgnoreCase))
            {
                if (!this.Options.TimeBasedKeyEnabled)
                {
                    return AuthenticateResult.Fail("Authentication type not supported.");
                }

                keyValidator = keyValidator ?? new TimeBasedApiKeyValidator(this.Clock, new Csg.AspNetCore.Authentication.ApiKey.TimeBasedTokenGenerator()
                {
                    IntervalSeconds = this.Options.TimeBasedKeyInterval,
                    NumberOfIntervalsOfTolerance = this.Options.TimeBasedKeyTolerance
                });
            }
            else if (keyValidator == null)
            {
                return AuthenticateResult.NoResult();
            }

            if (!(await keyValidator.ValidateKeyAsync(keyFromStore, requestMessage.Token)))
            {
                this.Logger.LogInformation("Key is not valid.");

                return AuthenticateResult.Fail("Invalid Key");
            }

            var principal = await CreateUserAsync(keyFromStore);
            
            //TODO: Hook for options to modify claims/user

            return AuthenticateResult.Success(new AuthenticationTicket(principal, this.Scheme.Name));
        }

        private async Task<System.Security.Claims.ClaimsPrincipal> CreateUserAsync(ApiKey key)
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

            return new System.Security.Claims.ClaimsPrincipal(identity);
        }

        //private async Task<KeyValidationContext> RunKeyValidatedEventAsync(ApiKey keyFromStore, ApiKey providedKey)
        //{
        //    var context = new KeyValidationContext() { KeyFromStore = keyFromStore, ProvidedKey = providedKey };

        //    await this.Options.Events.OnKeyValidatedAsync(context);

        //    return context;
        //}

        //private async Task<AuthenticatedContext> RunAuthenticatedEventAsync(ApiKey keyFromStore, ApiKey providedKey)
        //{
        //    var context = new AuthenticatedContext();

        //    await this.Options.Events.OnAuthenticatedAsync(context);

        //    return context;
        //}
    }
}
