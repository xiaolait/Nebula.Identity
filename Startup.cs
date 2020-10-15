using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nebula.Identity.Data;
using Nebula.Identity.Model;

namespace Nebula.Identity
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddDbContext<IdentityContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("mysql")));

            services.AddIdentity<User, Role>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                //https://docs.microsoft.com/zh-cn/aspnet/core/security/samesite?view=aspnetcore-3.1&viewFallbackFrom=aspnetcore-3
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            });

            var clients = new List<Client>();
            Configuration.GetSection("AuthClients").Bind(clients);

            /*
            clients.Add(new Client
            {
                ClientId = "pc",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword, //这里是指定授权的模式，选择密码模式，
                ClientSecrets = { new Secret("ci.nebula".Sha256()) },
                //RefreshTokenUsage = TokenUsage.ReUse,
                //AlwaysIncludeUserClaimsInIdToken = true,
                //AllowOfflineAccess = true,
                AllowedScopes = new List<string> {"openid", "profile", "api" },
                AccessTokenLifetime = 3600 * 24 * 30
            });
            */
            foreach(var client in clients)
            {
                if (client.AllowedGrantTypes.Contains("password")) client.ClientSecrets.Add(new Secret(client.ClientName.Sha256()));
            }
            

            services.AddIdentityServer(option=>option.IssuerUri="http://www.emnebula.com")
                .AddDeveloperSigningCredential()
                .AddInMemoryIdentityResources(new List<IdentityResource> {
                        new IdentityResources.OpenId(),
                        new IdentityResources.Profile(),
                        new IdentityResources.Email(),
                    })
                .AddInMemoryApiResources(new List<ApiResource> {
                        new ApiResource("api", "My API 1")
                    })
                .AddInMemoryClients(clients)
                .AddAspNetIdentity<User>();
            
            services.AddCors();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            
            app.UseDefaultFiles();
            app.UseStaticFiles();
            
            app.UseCors(builder => builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
                .AllowCredentials()
            );

            app.UseCookiePolicy();
            app.UseIdentityServer();

            //app.UseHttpsRedirection();
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
