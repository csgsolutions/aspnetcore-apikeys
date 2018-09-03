using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;

namespace ExampleAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info()
                {
                    Title = "ExampleAPI",
                    Version = "1.0"
                });

                c.AddSecurityDefinition("ApiKey", new ApiKeyScheme
                {
                    Description = "Authorization header using the API Key scheme. Example: \"Authorization: ApiKey {token}\"",
                    Name = "Authorization",
                    In = "header",
                    Type = "apiKey"
                });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>()
                {
                    { "ApiKey", new string[]{ } }
                });
            });


            services.AddConfigurationApiKeyStore(config =>
            {
                config.Keys = new Dictionary<string, string>()
                {
                    { "Key1", "secret1234" },
                    { "Key2", "secret4567" }
                };
            });

            services.AddAuthentication(Csg.AspNetCore.ApiKeyAuthentication.ApiKeyDefaults.Name).AddApiKey(config =>
            {

            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExampleAPI V1");
            });

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
