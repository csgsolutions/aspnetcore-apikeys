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
using Microsoft.OpenApi.Models;
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
            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo()
                {
                    Title = "ExampleAPI",
                    Version = "1.0"
                });

                var basicScheme = new Microsoft.OpenApi.Models.OpenApiSecurityScheme()
                {                    
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                    Scheme = "Basic",
                    In =  Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Username is ClientID, password is key.",
                    Reference = new OpenApiReference
                    {
                        Id = "Basic",
                        Type = ReferenceType.SecurityScheme
                    }
                };

                c.AddSecurityDefinition(basicScheme.Reference.Id, basicScheme);
                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
                {
                    { basicScheme, new string[]{ } }
                });
            });

            services.Configure<Csg.AspNetCore.Authentication.ApiKey.ConfigurationApiKeyStoreOptions>(this.Configuration.GetSection("ApiKeys"));

            services.AddConfigurationApiKeyStore();

            services.AddAuthentication(Csg.AspNetCore.Authentication.ApiKey.ApiKeyDefaults.Name)
                .AddApiKey(conf => {
                    conf.StaticKeyEnabled = true;
                    conf.HttpBasicEnabled = true;
                    conf.TimeBasedKeyEnabled = true;
                    
                    conf.HeaderName = "HeaderName";
                    conf.HeaderName = null;
                    conf.QueryString = "param_name";
                    conf.QueryString = null;
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

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ExampleAPI V1");
            });

            app.UseEndpoints(ep =>
            {
                ep.MapControllers();
            });
        }
    }
}
