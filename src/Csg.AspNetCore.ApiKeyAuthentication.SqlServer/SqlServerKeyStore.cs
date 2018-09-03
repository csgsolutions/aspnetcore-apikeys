using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Linq;
using System.Dynamic;

namespace Csg.AspNetCore.ApiKeyAuthentication.SqlServer
{
    public class SqlServerKeyStore : IApiKeyStore
    {
        private readonly string _connectionString;

        public bool EnableClaimsStore { get; set; } = false;

        public SqlServerKeyStore(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected async System.Threading.Tasks.Task<System.Data.SqlClient.SqlConnection> OpenConnectionAsync()
        {
            var conn = new System.Data.SqlClient.SqlConnection(_connectionString);

            await conn.OpenAsync();

            return conn;
        }

        public async Task<ApiKey> GetKeyAsync(string keyName)
        {
            using (var conn = await OpenConnectionAsync())
            {
                return await conn.QuerySingleAsync<ApiKey>("SELECT [KeyName], [Secret] FROM [ApiKey] WHERE [KeyName] = @keyName;", new
                {
                    keyName
                });
            }
        }

        public async Task<ICollection<Claim>> GetClaimsAsync(ApiKey key)
        {
            var defaultClaim = new System.Security.Claims.Claim(ClaimTypes.Name, key.ClientID);
            var claims = new List<Claim>();

            if (!this.EnableClaimsStore)
            {
                claims.Add(defaultClaim);
            }

            return claims;

            //using (var conn = await OpenConnectionAsync())
            //{
            //    return (await conn.QueryAsync<dynamic>("SELECT [Type], [Value] FROM [ApiKeyClaims] WHERE [KeyName] = @keyName;", new
            //    {
            //        key.KeyName
            //    }))
            //    .ToList();
            //}
        }        
    }
}
