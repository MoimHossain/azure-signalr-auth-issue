

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using WebApi.Hubs;


namespace WebApi
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
            services.AddApplicationInsightsTelemetry();
            services.AddAuthentication(sharedOptions =>
            {
                sharedOptions.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddAzureAdBearer(options => Configuration.Bind("AzureAd", options));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddAuthorization(configure =>
            {
                configure.AddPolicy("Custom-Policy", policy =>
                {
                    policy.RequireClaim("oid");
                    policy.RequireClaim("name");
                });
            });

            services.AddCors((corsOption) =>
            {
                corsOption
                    .AddPolicy("cors-development",
                        pb =>
                            pb
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowAnyOrigin()
                            .AllowCredentials());
                corsOption
                    .AddPolicy("cors-production",
                        pb =>
                            pb
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .SetIsOriginAllowed(domain => new[] { "<YOUR AZURE WEB APP>.azurewebsites.net" }.Contains(domain))
                            .AllowCredentials());
            });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services
                .AddSignalR()
                .AddAzureSignalR(options =>
            {
                // this piece of code is needed for Azure SignalR to work
                options.ClaimsProvider = context =>
                { 
                    return context.User.Claims;
                };
            });
            //services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            if (env.IsDevelopment())
            {
                app.UseCors("cors-development");
            }
            else
            {
                app.UseHsts();
                app.UseCors("cors-production");
            }

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseFileServer();
            app.UseAzureSignalR(routes =>
            //app.UseSignalR(routes =>
            {
                routes.MapHub<TaskHub>("/taskhub");
            });
            app.UseMvc();
        }
    }
}
