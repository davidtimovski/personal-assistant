using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PersonalAssistant.Application;
using PersonalAssistant.Application.Contracts.Accountant.Categories;
using PersonalAssistant.Application.Contracts.Accountant.Common;
using PersonalAssistant.Application.Contracts.Accountant.Transactions;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using PersonalAssistant.Infrastructure;
using PersonalAssistant.Persistence;
using PersonalAssistant.Persistence.Repositories.Accountant;
using PersonalAssistant.Persistence.Repositories.Common;
using PersonalAssistant.Persistence.Repositories.ToDoAssistant;
using Serilog;

namespace PersonalAssistant.WorkerService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("appsettings.json", false);
                    config.AddJsonFile($"appsettings.{environmentName}.json", true, true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((hostContext, logging) =>
                {
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .CreateLogger();

                    logging.AddSerilog(Log.Logger, dispose: true);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddInfrastructure(hostContext.Configuration, hostContext.HostingEnvironment.EnvironmentName);
                    services.AddPersistence(hostContext.Configuration);
                    services.AddApplication(hostContext.Configuration);

                    services.AddOptions();
                    services.Configure<DatabaseSettings>(x => x.DefaultConnectionString = hostContext.Configuration.GetValue<String>("ConnectionString"));
                    services.AddHttpClient("fixer", c =>
                    {
                        c.BaseAddress = new Uri("http://data.fixer.io/api/");
                    });

                    services.AddTransient<INotificationsRepository, NotificationsRepository>();
                    services.AddTransient<IDeletedEntitiesRepository, DeletedEntitiesRepository>();
                    services.AddTransient<IUpcomingExpensesRepository, UpcomingExpensesRepository>();
                    services.AddTransient<ICategoriesRepository, CategoriesRepository>();
                    services.AddTransient<ITransactionsRepository, TransactionsRepository>();
                    services.AddTransient<ICurrencyRatesRepository, CurrencyRatesRepository>();

                    services.AddHostedService<MidnightWorker>();
                });
    }
}
