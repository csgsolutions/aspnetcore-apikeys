using System;
using System.Collections.Generic;
using System.Text;
using Csg.AspNetCore.Authentication.ApiKey;

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
            TimeBasedTokenGenerator tokenGenerator = null,
            string authenticationType = "TApiKey")
        {
            client.DefaultRequestHeaders.AddApiKeyAuthorizationHeader(clientID, secret, utcNow, tokenGenerator, authenticationType);  
        }

        public static void AddApiKeyAuthorizationHeader(this System.Net.Http.Headers.HttpRequestHeaders headers, 
            string clientID, 
            string secret, 
            DateTimeOffset utcNow, 
            TimeBasedTokenGenerator tokenGenerator = null,
            string authenticationType = "TApiKey")
        {
            tokenGenerator = tokenGenerator ?? _defaultTokenGenerator;

            if (headers.Contains(Authorization))
            {
                headers.Remove(Authorization);
            }

            var token = tokenGenerator.GenerateToken(clientID, secret, utcNow);

            token = System.Net.WebUtility.UrlEncode(token);

            headers.Add("Authorization", $"{authenticationType} {clientID}:{token}");
        }

        public static void AddApiKeyAuthorizationHeader(this System.Net.Http.Headers.HttpRequestHeaders headers, string clientID, string secret, string authenticationType = "ApiKey")
        {
            if (headers.Contains(Authorization))
            {
                headers.Remove(Authorization);
            }

            secret = System.Net.WebUtility.UrlEncode(secret);

            headers.Add("Authorization", $"{authenticationType} {clientID}:{secret}");
        }

        public static void AddApiKeyAuthorizationHeader(this System.Net.Http.HttpClient client,
            string clientID,
            string secret,
            string authenticationType = "ApiKey")
        {
            client.DefaultRequestHeaders.AddApiKeyAuthorizationHeader(clientID, secret, authenticationType);
        }

    }
}
