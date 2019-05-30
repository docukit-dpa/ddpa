using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Core;
using OpenIddict.Models;
using DDPA.Attributes;
using DDPA.Service;
using DDPA.Validation;
using DDPA.SQL.Entities;
using DDPA.SQL.Repositories;
using DDPA.SQL.Repositories.Context;
using DDPA.Web.TokenProvider;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DDPA.Commons.Settings;
using Serilog;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Pomelo.EntityFrameworkCore.MySql;
using MySql.Data.MySqlClient;
using DDPA.DTO;

namespace DDPA.Web
{
    public class Startup
    {
        private IHostingEnvironment _env;
        private const string EmailConfirmationTokenProviderName = "ConfirmEmail";

        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            _env = env;
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                //builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder => builder.AllowAnyOrigin());

                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder.WithOrigins("http://localhost"));
            });

            services.Configure<SMTPOptions>(Configuration.GetSection("SmtpOptions"));
            //services
            //    .AddMvc()
            //    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

            //// Localization
            //services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddOpenIddict()
                .AddEntityFrameworkCoreStores<ApplicationDbContext>();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                // Configure the context to use Microsoft SQL Server.
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"));
                // Register the entity sets needed by OpenIddict.
                // Note: use the generic overload if you need
                // to replace the default OpenIddict entities.
                options.UseOpenIddict();
            });

            var attachmentPath = Configuration.GetSection("DDPAOptions:AttachmentPath").Value;
            if (!Directory.Exists(attachmentPath))
                Directory.CreateDirectory(attachmentPath);
            // Register the Identity services.
            services.AddIdentity<ExtendedIdentityUser, IdentityRole<int>>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders()
                .AddTokenProvider<ConfirmEmailDataProtectorTokenProvider<ExtendedIdentityUser>>(EmailConfirmationTokenProviderName);

            services.Configure<IdentityOptions>(o =>
            {
                o.Password.RequiredLength = 8;
                o.Password.RequireDigit = true;
                o.Password.RequireLowercase = true;
                o.Password.RequireUppercase = true;
                o.Password.RequireNonAlphanumeric = false;

                o.ClaimsIdentity.UserNameClaimType = OpenIdConnectConstants.Claims.Name;
                o.ClaimsIdentity.UserIdClaimType = OpenIdConnectConstants.Claims.Subject;
                o.ClaimsIdentity.RoleClaimType = OpenIdConnectConstants.Claims.Role;

                // Token Provider
                o.Tokens.EmailConfirmationTokenProvider = EmailConfirmationTokenProviderName;
            });

            services.AddDistributedMemoryCache();
            // Add Session
            services.AddSession(options =>
            {
                options.Cookie.Name = ".DDPASessionCookies";
                options.IdleTimeout = TimeSpan.FromHours(1);
                //options.IdleTimeout = TimeSpan.FromSeconds(10);
                //options.Cookie.HttpOnly = true;
            });

            services.AddAuthentication();

            // Localization
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                };

                // State what the default culture for your application is. This will be used if no specific culture
                // can be determined for a given request.
                options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");

                // You must explicitly state which cultures your application supports.
                // These are the cultures the app supports for formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;

                // These are the cultures the app supports for UI strings, i.e. we have localized resources for.
                options.SupportedUICultures = supportedCultures;

            });
            services.AddDistributedMemoryCache();
            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            var location = (string)Configuration.GetSection("DataProtection").GetValue(typeof(string), "Directory");
            var pathToCryptoKeys = Path.Combine(location, "dp_keys");
            services.AddDataProtection()
                .SetApplicationName("DDPA web") // do not change this
                .PersistKeysToFileSystem(new DirectoryInfo(pathToCryptoKeys));

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            });

            services.AddMemoryCache();

            // Main Repository
            services.AddScoped<IRepository, EFRepository<ApplicationDbContext>>();

            services.AddScoped<SharedMessageAttribute>();

            // Services            
            services.AddScoped<IAccountService, AccountService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IQueryService, QueryService>();
            services.AddScoped<IMaintenanceService, MaintenanceService>();
            services.AddScoped<IUserValidation, UserValidation>();
            services.AddScoped<IValidationService, ValidationService>();
            services.Configure<DDPAOptions>(Configuration.GetSection("DDPAOptions"));
            services.AddScoped<IDatasetService, DatasetService>();
            services.AddScoped<ISummaryService, SummaryService>();
            services.AddScoped<IApprovalService, ApprovalService>();
            services.AddScoped<IResourceService, ResourceService>();
            services.AddScoped<ISupportService, SupportService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            // Ensure any buffered events are sent at shutdown
            //appLifetime.ApplicationStopped.Register(Log.CloseAndFlush);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Error/Error");
                app.UseStatusCodePagesWithReExecute("/Error/Errors/{0}");
            }

            // enable session
            app.UseSession();

            app.UseStaticFiles(new StaticFileOptions()
            {
                ServeUnknownFileTypes = true,
                DefaultContentType = "application/octet-stream"
            });

            app.UseStatusCodePagesWithReExecute("/error");

            app.UseAuthentication();

            app.UseCors("AllowAllOrigins");
            app.UseCors("AllowSpecificOrigin");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Account}/{action=Login}/{id?}");
            });
            // Seed the database with the sample applications.
            // Note: in a real world application, this step should be part of a setup script.
            InitializeAsync(app.ApplicationServices, CancellationToken.None).GetAwaiter().GetResult();

        }
        private async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken)
        {
            // Create a new service scope to ensure the database context is correctly disposed when this methods returns.
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await context.Database.EnsureCreatedAsync();
            }
        }
    }
}
