using System;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nebula.Identity.Model;
using System.Security.Claims;
using IdentityModel;

namespace Nebula.Identity
{
    public static class WebHostExtensions
    {
        public static IWebHost MigrateDbContext<TContext>(this IWebHost host) where TContext : DbContext
        {
            using(var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var context = services.GetService<TContext>();

                try {
                    context.Database.Migrate();
                    logger.LogInformation("DbContext Init success!");
                } catch (Exception ex) {
                    logger.LogInformation("DbContext Init failed!");
                }
            }
            using(var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<TContext>>();
                var userManager = services.GetRequiredService<UserManager<User>>();

                try {
                    var templateUser = userManager.Users.Where(u => u.UserName == "template").FirstOrDefault();
                    if (templateUser == null) {
                        templateUser = new User() {UserName = "template"};
                        userManager.CreateAsync(templateUser, "123456789aS");
                        userManager.AddClaimAsync(templateUser, new Claim(JwtClaimTypes.NickName, "模版")).Wait();
                    }
                    var exampleUser = userManager.Users.Where(u => u.UserName == "example").FirstOrDefault();
                    if (exampleUser == null) {
                        exampleUser = new User() {UserName = "example"};
                        userManager.CreateAsync(exampleUser, "123456789aS");
                        userManager.AddClaimAsync(exampleUser, new Claim(JwtClaimTypes.NickName, "示例")).Wait();
                    }
                    logger.LogInformation("Users Init success!");
                } catch (Exception ex) {
                    logger.LogInformation("Users Init failed!");
                }
            }
            return host;
        }
    }
}