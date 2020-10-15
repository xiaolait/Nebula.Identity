using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nebula.Identity.Model;

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
                    //Task.Delay(8000).Wait();
                    context.Database.Migrate();
                    logger.LogInformation("DbContext Init success!");
                } catch (Exception ex) {
                    logger.LogInformation("DbContext Init failed!");
                }
                return host;
            }
        }
    }
}