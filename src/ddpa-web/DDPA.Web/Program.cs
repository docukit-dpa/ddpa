using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DDPA.Service;
using DDPA.SQL.Entities;
using DDPA.SQL.Repositories.Context;
using DDPA.Web.Data;
using Serilog;

namespace DDPA.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
                    var userManager = services.GetRequiredService<UserManager<ExtendedIdentityUser>>();
                    var hosting = services.GetRequiredService<IHostingEnvironment>();
                    var adminService = services.GetRequiredService<IAdminService>();
                    var maintenanceService = services.GetRequiredService<IMaintenanceService>();

                    DbInitializer.Seed(context, roleManager, userManager, hosting, adminService, maintenanceService).Wait();
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                .ReadFrom.Configuration(hostingContext.Configuration)
                .WriteTo.Console())
                .Build();
    }
}
