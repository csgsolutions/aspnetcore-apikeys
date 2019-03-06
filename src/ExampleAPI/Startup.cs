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

                //c.AddSecurityDefinition("ApiKey", new ApiKeyScheme
                //{
                //    Description = "Authorization header using the API Key scheme. Example: \"Authorization: ApiKey clientID:secret\"",
                //    Name = "Authorization",
                //    In = "header",
                //    Type = "apiKey"
                //});

                //c.AddSecurityDefinition("TApiKey", new ApiKeyScheme
                //{
                //    Description = "Authorization header using the time-based API Key scheme. Example: \"Authorization: TApiKey clientID:token\"",
                //    Name = "Authorization",
                //    In = "header",
                //    Type = "apiKey"
                //});

                c.AddSecurityDefinition("Basic", new Swashbuckle.AspNetCore.Swagger.BasicAuthScheme()
                {
                    Description = "Username is ClientID, password is key."
                });

                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>()
                {
                    { "Basic", new string[]{ } }
                });
            });

            services.Configure<Csg.AspNetCore.Authentication.ApiKey.ConfigurationApiKeyStoreOptions>(this.Configuration.GetSection("ApiKeys"));

            services.AddConfigurationApiKeyStore();

            services.AddAuthentication(Csg.AspNetCore.Authentication.ApiKey.ApiKeyDefaults.Name).AddApiKey(conf =>
            {
                conf.HttpBasicEnabled = false;
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
