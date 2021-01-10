using System;
using System.Globalization;
using Api.Config;
using AutoMapper;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalAssistant.Application;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using PersonalAssistant.Infrastructure;
using PersonalAssistant.Persistence;

namespace Api
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

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = Configuration["Urls:Authority"];
#if DEBUG
                    options.RequireHttpsMetadata = false;
#endif
                    options.Audience = "personal-assistant-api";
                });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowToDoAssistant", builder =>
                {
                    builder.WithOrigins(Configuration["Urls:ToDoAssistant"])
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .SetPreflightMaxAge(TimeSpan.FromSeconds(1728000)); // 20 days
                });

                options.AddPolicy("AllowCookingAssistant", builder =>
                {
                    builder.WithOrigins(Configuration["Urls:CookingAssistant"])
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .SetPreflightMaxAge(TimeSpan.FromSeconds(1728000)); // 20 days
                });

                options.AddPolicy("AllowAccountant", builder =>
                {
                    builder.WithOrigins(Configuration["Urls:Accountant"])
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .SetPreflightMaxAge(TimeSpan.FromSeconds(1728000)); // 20 days
                });

                options.AddPolicy("AllowAllApps", builder =>
                {
                    builder.WithOrigins(Configuration["Urls:ToDoAssistant"],
                                        Configuration["Urls:CookingAssistant"],
                                        Configuration["Urls:Accountant"])
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .SetPreflightMaxAge(TimeSpan.FromSeconds(1728000)); // 20 days
                });
            });

            services.AddApplicationInsightsTelemetry();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddLocalization(options => options.ResourcesPath = "Resources");
            services.AddMvc(options =>
                {
                    options.EnableEndpointRouting = false;
                })
                .AddNewtonsoftJson()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddHttpClient();
            services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

            services.Configure<Urls>(Configuration.GetSection("Urls"));
            services.Configure<DailyIntakeReference>(Configuration.GetSection("DietaryProfile:ReferenceDailyIntake"));
        }

        public void Configure(IApplicationBuilder app)
        {
            if (WebHostEnvironment.EnvironmentName == "Development")
            {
                var telemetryConfiguration = app.ApplicationServices.GetService<TelemetryConfiguration>();
                telemetryConfiguration.DisableTelemetry = true;

                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionHandler("/error");

            var supportedCultures = new[] {
                new CultureInfo("en-US"),
                new CultureInfo("mk-MK")
            };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(culture: supportedCultures[0].Name, uiCulture: supportedCultures[0].Name),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            });

            app.UseCors("AllowAllApps");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMvc();
        }
    }
}
