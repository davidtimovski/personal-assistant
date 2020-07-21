using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalAssistant.Application.Contracts.Accountant.Accounts;
using PersonalAssistant.Application.Contracts.Accountant.Categories;
using PersonalAssistant.Application.Contracts.Accountant.Common;
using PersonalAssistant.Application.Contracts.Accountant.Debts;
using PersonalAssistant.Application.Contracts.Accountant.Transactions;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Persistence.Repositories.Accountant;
using PersonalAssistant.Persistence.Repositories.Common;
using PersonalAssistant.Persistence.Repositories.CookingAssistant;
using PersonalAssistant.Persistence.Repositories.ToDoAssistant;

namespace PersonalAssistant.Persistence
{
    public static class IoC
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IUsersRepository, UsersRepository>();
            services.AddTransient<IPushSubscriptionsRepository, PushSubscriptionsRepository>();
            services.AddTransient<ITooltipsRepository, TooltipsRepository>();
            services.AddTransient<IListsRepository, ListsRepository>();
            services.AddTransient<ITasksRepository, TasksRepository>();
            services.AddTransient<INotificationsRepository, NotificationsRepository>();
            services.AddTransient<IRecipesRepository, RecipesRepository>();
            services.AddTransient<IIngredientsRepository, IngredientsRepository>();
            services.AddTransient<IDietaryProfilesRepository, DietaryProfilesRepository>();
            services.AddTransient<IAccountsRepository, AccountsRepository>();
            services.AddTransient<ICategoriesRepository, CategoriesRepository>();
            services.AddTransient<ITransactionsRepository, TransactionsRepository>();
            services.AddTransient<IUpcomingExpensesRepository, UpcomingExpensesRepository>();
            services.AddTransient<IDebtsRepository, DebtsRepository>();
            services.AddTransient<IUnitOfWork, UnitOfWork>();

            services.Configure<DatabaseSettings>(options =>
            {
                options.DefaultConnectionString = configuration["ConnectionStrings:DefaultConnection"];
            });

            return services;
        }
    }
}