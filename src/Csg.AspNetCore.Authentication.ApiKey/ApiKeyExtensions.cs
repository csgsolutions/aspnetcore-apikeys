using Csg.AspNetCore.Authentication.ApiKey;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApiKeyExtensions
    {
        public static void AddConfigurationApiKeyStore(this IServiceCollection services, Action<ConfigurationApiKeyStoreOptions> setupAction = null)
        {
            if (setupAction != null)
            {
                services.Configure<ConfigurationApiKeyStoreOptions>(setupAction);
            }

            services.TryAddSingleton<IApiKeyStore, ConfigurationApiKeyStore>();
        }

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder)
        {
            return builder.AddApiKey(ApiKeyDefaults.Name, _ => { });
        }

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, Action<ApiKeyOptions> configureOptions)
        {
            return builder.AddApiKey(ApiKeyDefaults.Name, configureOptions);
        }

        public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, string authenticationScheme, Action<ApiKeyOptions> configureOptions)
        {
            return builder.AddScheme<ApiKeyOptions, ApiKeyHandler>(authenticationScheme, configureOptions);
        }
    }
}
