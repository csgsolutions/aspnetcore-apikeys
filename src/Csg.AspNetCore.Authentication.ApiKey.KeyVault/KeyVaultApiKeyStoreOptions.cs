namespace Csg.AspNetCore.Authentication.ApiKey.KeyVault
{
    public class KeyVaultApiKeyStoreOptions
    {
        public string KeyVaultUrl { get; set; }
        public string ClientPrefix { get; set; }
        public int CacheTimeToLiveMinutes { get; set; } = 90;
    }    
}
