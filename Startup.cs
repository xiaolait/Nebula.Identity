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

            var clientUrls = new List<string>();
            for (int i=1; i<=10; i++)
            {
                var clientUrl = Configuration[$"ClientUrl{i}"];
                if (!string.IsNullOrEmpty(clientUrl)) clientUrls.Add(clientUrl);
            }
            clients.ForEach(c => {
                if (c.AllowedGrantTypes.Contains("implicit")) clientUrls.ForEach(url => {
                    c.RedirectUris.Add($"{url}/CallBack");
                    c.PostLogoutRedirectUris.Add(url);
                    c.AllowedCorsOrigins.Add(url);
                });
            });

            /*
            var authClients = new List<AuthClient>();
            for (int i=0; i<10; i++)
            {
                var clientId = Configuration[$"ClientId{i}"];
                if (string.IsNullOrEmpty(clientId)) continue;
                var authClient = new AuthClient{
                    ClientId = clientId,
                    AllowedGrantTypes = Configuration[$"AllowedGrantTypes{i}"] ?? "password",
                    ClientUrl = Configuration[$"ClientUrl{i}"],
                    AccessTokenLifetime = string.IsNullOrEmpty(Configuration[$"AccessTokenLifetime{i}"]) ? 2592000 : int.Parse(Configuration[$"AccessTokenLifetime{i}"])
                };
                authClients.Add(authClient);
            }
            authClients.ForEach(c => {
                var client = new Client{
                    ClientId = c.ClientId,
                    ClientName = c.ClientId,
                    AllowedGrantTypes = new string[] { c.AllowedGrantTypes },
                    AccessTokenLifetime = c.AccessTokenLifetime,
                    AllowedScopes = new string[]{ "openid", "profile", "api" },
                    AllowAccessTokensViaBrowser = true,
                    RequireConsent = false
                };
                if (!string.IsNullOrEmpty(c.ClientUrl)) {
                    client.RedirectUris = new string[] {$"{c.ClientUrl}/CallBack"};
                    client.PostLogoutRedirectUris = new string[] {c.ClientUrl};
                    client.AllowedCorsOrigins = new string[] {c.ClientUrl};
                }
                clients.Add(client);
            });

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
