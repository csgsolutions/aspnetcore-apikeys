using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Linq;
using System.Dynamic;

namespace Csg.AspNetCore.Authentication.ApiKey.SqlServer
{
    public class SqlServerKeyStore : IApiKeyStore
    {
        private readonly string _connectionString;

        public bool EnableClaimsStore { get; set; } = false;

        public bool SupportsClaims => this.EnableClaimsStore;

        public string SelectKeyByClientIdQuery { get; set; } = "SELECT [ClientID], [Secret] FROM [ApiKey] WHERE [ClientID] = @clientID;";

        public string SelectClaimsByClientIdQuery { get; set; } = "";
 
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

        public async Task<ApiKey> GetKeyAsync(string clientID)
        {
            using (var conn = await OpenConnectionAsync())
            {
                return await conn.QuerySingleAsync<ApiKey>(this.SelectKeyByClientIdQuery, new
                {
                    clientID
                });
            }
        }

        public async Task<ICollection<Claim>> GetClaimsAsync(ApiKey key)
        {
            throw new NotImplementedException();

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
