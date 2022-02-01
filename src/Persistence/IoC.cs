using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Application.Contracts.Accountant.Accounts;
using Application.Contracts.Accountant.Categories;
using Application.Contracts.Accountant.Common;
using Application.Contracts.Accountant.Debts;
using Application.Contracts.Accountant.Transactions;
using Application.Contracts.Accountant.UpcomingExpenses;
using Application.Contracts.Common;
using Application.Contracts.CookingAssistant.DietaryProfiles;
using Application.Contracts.CookingAssistant.Ingredients;
using Application.Contracts.CookingAssistant.Recipes;
using Application.Contracts.ToDoAssistant.Lists;
using Application.Contracts.ToDoAssistant.Notifications;
using Application.Contracts.ToDoAssistant.Tasks;
using Persistence.Repositories.Accountant;
using Persistence.Repositories.Common;
using Persistence.Repositories.CookingAssistant;
using Persistence.Repositories.ToDoAssistant;

namespace Persistence
{
    public static class IoC
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, string connectionString)
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

            services.AddDbContext<PersonalAssistantContext>(options =>
            {
                options.UseNpgsql(connectionString);
            });

            return services;
        }
    }
}