using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Csg.AspNetCore.Authentication.ApiKey;
using Microsoft.Extensions.Options;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;

namespace Csg.AspNetCore.Authentication.ApiKey.KeyVault
{
    public class KeyVaultApiKeyStore : Csg.AspNetCore.Authentication.ApiKey.IApiKeyStore
    {
        private static readonly AzureServiceTokenProvider s_azureServiceTokenProvider = new AzureServiceTokenProvider();
        private static readonly KeyVaultClient s_keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(s_azureServiceTokenProvider.KeyVaultTokenCallback));

        private readonly KeyVaultApiKeyStoreOptions _options;
        
        private readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);
        private readonly System.Threading.SemaphoreSlim _lock = new System.Threading.SemaphoreSlim(1, 1);
        
        public KeyVaultApiKeyStore(IOptions<KeyVaultApiKeyStoreOptions> options)
        {
            _options = options.Value;
        }

        public bool SupportsClaims => false;

        public Task<ICollection<Claim>> GetClaimsAsync(ApiKey key)
        {
            throw new NotImplementedException();
        }
               
        public async Task<ApiKey> GetKeyAsync(string clientID)
        {
            if (TryGetFromCache(clientID, out ApiKey key))
            {
                return key;
            }

            await _lock.WaitAsync().ConfigureAwait(false);

            try
            {
                // do this again because it may have been added before we aquired the lock
                if (TryGetFromCache(clientID, out key))
                {
                    return key;
                }

                // get the secret from the vault
                string secretValue = await GetKeyFromVaultAsync(string.Concat(_options.ClientPrefix, clientID)).ConfigureAwait(false);

                if (secretValue == null)
                {
                    return null;
                }

                // cache the secret we just got
                AddToCache(clientID, secretValue);

                return new ApiKey()
                {
                    Secret = secretValue,
                    ClientID = clientID
                };
            }
            finally
            {
                _lock.Release();
            }
        }    

        private async Task<string> GetKeyFromVaultAsync(string secretName)
        {
            var secret = await s_keyVaultClient.GetSecretAsync($"{_options.KeyVaultUrl}secrets/{secretName}").ConfigureAwait(false);
            
            if (secret == null)
            {
                return null;
            }

            return secret.Value;
        }

        private bool TryGetFromCache(string clientID, out ApiKey key)
        {
            key = null;

            if (_cache.TryGetValue(clientID, out CacheEntry secret))
            {
                if (secret.Expires < DateTime.UtcNow)
                {
                    return false;
                }

                key = new ApiKey()
                {
                    ClientID = clientID,
                    Secret = secret.Secret
                };

                return true;
            }

            return false;
        }

        private void AddToCache(string clientID, string secret)
        {
            _cache.Add(clientID, new CacheEntry()
            {
                Secret = secret,
                Expires = DateTime.UtcNow.AddMinutes(_options.CacheTimeToLiveMinutes)
            });
        }

        private class CacheEntry
        {
            public string Secret { get; set; }
            public DateTime Expires { get; set; }
        }
    }
}
