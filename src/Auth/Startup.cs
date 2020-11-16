using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using FluentValidation.AspNetCore;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
#if !DEBUG
using Microsoft.AspNetCore.HttpOverrides;
#endif
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Auth.Models;
using Auth.Models.Config;
using PersonalAssistant.Infrastructure;
using Serilog;
using PersonalAssistant.Infrastructure.Identity;
using PersonalAssistant.Persistence;
using PersonalAssistant.Application;
using Auth.Services;

namespace Auth
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        public IConfiguration Configuration { get; }
        private IWebHostEnvironment WebHostEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddInfrastructure(Configuration, WebHostEnvironment.EnvironmentName);
            services.AddPersistence(Configuration);
            services.AddApplication(Configuration);

            services.AddDbContext<PersonalAssistantAuthContext>(options =>
            {
                options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddIdentity<ApplicationUser, IdentityRole<int>>(config =>
            {
                config.Password.RequiredLength = 8;
                config.Password.RequireDigit = false;
                config.Password.RequireLowercase = false;
                config.Password.RequireUppercase = false;
                config.Password.RequireNonAlphanumeric = false;
                config.SignIn.RequireConfirmedEmail = true;
                config.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            })
                .AddEntityFrameworkStores<PersonalAssistantAuthContext>()
                .AddDefaultTokenProviders();

            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<IdentityServerProfileService>()
                .AddConfigurationStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseNpgsql(Configuration["ConnectionStrings:DefaultConnection"],
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                .AddOperationalStore(options =>
                {
                    options.ConfigureDbContext = builder =>
                        builder.UseNpgsql(Configuration["ConnectionStrings:DefaultConnection"],
                            sql => sql.MigrationsAssembly(migrationsAssembly));

                    // This enables automatic token cleanup. This is optional.
                    options.EnableTokenCleanup = true;
                    options.TokenCleanupInterval = 30;
                })
                .AddSigningCredential(new X509Certificate2(Directory.GetCurrentDirectory() + "/" + Configuration["Certificate:Name"], Configuration["Certificate:Password"]));

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Configuration["AuthKeysDirectory"]))
                .ProtectKeysWithCertificate(new X509Certificate2(Directory.GetCurrentDirectory() + "/" + Configuration["Certificate:Name"], Configuration["Certificate:Password"]));

            services.AddAuthentication();

            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });

            services.AddApplicationInsightsTelemetry();
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddCors();
            services.AddMvc(options =>
                {
                    options.EnableEndpointRouting = false;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddViewLocalization()
                .AddFluentValidation(x => x.RegisterValidatorsFromAssemblyContaining<Startup>());
            services.AddHttpClient();
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .CreateLogger();

            services.AddTransient<IEmailTemplateService, EmailTemplateService>();

            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            services.Configure<DatabaseSettings>(Configuration.GetSection("ConnectionStrings"));
        }

        public void Configure(IApplicationBuilder app)
        {
            if (WebHostEnvironment.EnvironmentName == "Development")
            {
                var telemetryConfiguration = app.ApplicationServices.GetService<TelemetryConfiguration>();
                telemetryConfiguration.DisableTelemetry = true;
                app.UseDeveloperExceptionPage();
            }

#if !DEBUG
            app.Use((context, next) =>
            {
                context.Request.Scheme = "https";
                return next();
            });

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
#endif

            var supportedCultures = new[] {
                new CultureInfo("en-US"), // Default
                new CultureInfo("mk-MK")
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(culture: supportedCultures[0].Name, uiCulture: supportedCultures[0].Name),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.UseCors(builder =>
            {
                builder.WithOrigins(
                    Configuration["AppSettings:Urls:ToDoAssistant"],
                    Configuration["AppSettings:Urls:CookingAssistant"],
                    Configuration["AppSettings:Urls:Accountant"]
                );
                builder.WithMethods("GET");
                builder.WithHeaders("Authorization");
            });

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    ctx.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age={60 * 60 * 24 * 7}";
                }
            });
            app.UseCookiePolicy();
            app.UseStatusCodePagesWithReExecute("/error", "?code={0}");

            app.UseMvcWithDefaultRoute();

            //InitializeDatabase(app);
        }

        private void InitializeDatabase(IApplicationBuilder app)
        {
            using var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
            serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();

            var context = serviceScope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            context.Database.Migrate();
            if (!context.Clients.Any())
            {
                foreach (var client in IdentityServerConfig.GetClients(Configuration))
                {
                    context.Clients.Add(client.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in IdentityServerConfig.IdentityResources)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in IdentityServerConfig.ApiResources)
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                context.SaveChanges();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var scope in IdentityServerConfig.ApiScopes)
                {
                    context.ApiScopes.Add(scope.ToEntity());
                }
                context.SaveChanges();
            }
        }
    }
}
