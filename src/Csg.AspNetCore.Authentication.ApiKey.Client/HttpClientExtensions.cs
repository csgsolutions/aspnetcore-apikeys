using System;
using System.Collections.Generic;
using System.Text;
using Csg.ApiKeyGenerator;

namespace System.Net.Http
{
    public static class HttpClientExtensions
    {
        private const string Authorization = "Authorization";
        private static readonly TimeBasedTokenGenerator _defaultTokenGenerator = new TimeBasedTokenGenerator();

        public static void AddApiKeyAuthorizationHeader(this System.Net.Http.HttpClient client, 
            string clientID, 
            string secret, 
            DateTimeOffset utcNow,
            TimeBasedTokenGenerator tokenGenerator = null)
        {
            client.DefaultRequestHeaders.AddApiKeyAuthorizationHeader(clientID, secret, utcNow, tokenGenerator);  
        }

        public static void AddApiKeyAuthorizationHeader(this System.Net.Http.Headers.HttpRequestHeaders headers, 
            string clientID, 
            string secret, 
            DateTimeOffset utcNow, 
            TimeBasedTokenGenerator tokenGenerator = null)
        {
            tokenGenerator = tokenGenerator ?? _defaultTokenGenerator;

            string token = Microsoft.AspNetCore.WebUtilities.Base64UrlTextEncoder.Encode(tokenGenerator.ComputeToken(clientID, secret, utcNow));

            headers.Add("Authorization", $"TAPIKEY {clientID}:{token}");
        }

        public static void AddApiKeyAuthorizationHeader(this System.Net.Http.Headers.HttpRequestHeaders headers, string clientID, string secret)
        {
            secret = System.Net.WebUtility.UrlEncode(secret);
            headers.Add("Authorization", $"APIKEY {clientID}:{secret}");
        }

        public static void AddApiKeyAuthorizationHeader(this System.Net.Http.HttpClient client,
            string clientID,
            string secret)
        {
            client.DefaultRequestHeaders.AddApiKeyAuthorizationHeader(clientID, secret);
        }

    }
}
