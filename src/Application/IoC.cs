using System.Collections.Generic;
using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Contracts.Accountant.Accounts;
using Application.Contracts.Accountant.Categories;
using Application.Contracts.Accountant.Debts;
using Application.Contracts.Accountant.Transactions;
using Application.Contracts.Accountant.UpcomingExpenses;
using Application.Contracts.Accountant.Sync;
using Application.Contracts.Common;
using Application.Contracts.Common.Models;
using Application.Contracts.CookingAssistant.Common;
using Application.Contracts.CookingAssistant.DietaryProfiles;
using Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using Application.Contracts.CookingAssistant.Ingredients;
using Application.Contracts.CookingAssistant.Ingredients.Models;
using Application.Contracts.CookingAssistant.Recipes;
using Application.Contracts.CookingAssistant.Recipes.Models;
using Application.Contracts.ToDoAssistant.Lists;
using Application.Contracts.ToDoAssistant.Lists.Models;
using Application.Contracts.ToDoAssistant.Notifications;
using Application.Contracts.ToDoAssistant.Tasks;
using Application.Contracts.ToDoAssistant.Tasks.Models;
using Application.Services.Accountant;
using Application.Services.Common;
using Application.Services.CookingAssistant;
using Application.Services.ToDoAssistant;
using Utility;

namespace Application
{
    public static class IoC
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            services.AddTransient<IConversion, Conversion>();

            services.AddTransient<IValidator<CreateList>, CreateListValidator>();
            services.AddTransient<IValidator<UpdateList>, UpdateListValidator>();
            services.AddTransient<IValidator<ShareList>, ShareListValidator>();
            services.AddTransient<IValidator<CopyList>, CopyListValidator>();
            services.AddTransient<IValidator<UpdateSharedList>, UpdateSharedListValidator>();
            services.AddTransient<IValidator<CreateTask>, CreateTaskValidator>();
            services.AddTransient<IValidator<BulkCreate>, BulkCreateValidator>();
            services.AddTransient<IValidator<UpdateTask>, UpdateTaskValidator>();
            services.AddTransient<IValidator<CreateRecipe>, CreateRecipeValidator>();
            services.AddTransient<IValidator<UpdateRecipe>, UpdateRecipeValidator>();
            services.AddTransient<IValidator<ShareRecipe>, ShareRecipeValidator>();
            services.AddTransient<IValidator<CreateSendRequest>, CreateSendRequestValidator>();
            services.AddTransient<IValidator<ImportRecipe>, ImportRecipeValidator>();
            services.AddTransient<IValidator<UpdateIngredient>, UpdateIngredientValidator>();
            services.AddTransient<IValidator<GetRecommendedDailyIntake>, GetRecommendedDailyIntakeValidator>();
            services.AddTransient<IValidator<UpdateDietaryProfile>, UpdateDietaryProfileValidator>();
            services.AddTransient<IValidator<UploadTempImage>, UploadTempImageValidator>();

            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IPushSubscriptionService, PushSubscriptionService>();
            services.AddTransient<ITooltipService, TooltipService>();
            services.AddTransient<IListService, ListService>();
            services.AddTransient<ITaskService, TaskService>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<IRecipeService, RecipeService>();
            services.AddTransient<IIngredientService, IngredientService>();
            services.AddTransient<IDietaryProfileService, DietaryProfileService>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<IUpcomingExpenseService, UpcomingExpenseService>();
            services.AddTransient<IDebtService, DebtService>();
            services.AddTransient<ISyncService, SyncService>();

            var activityMultiplier = new Dictionary<string, float>();
            configuration.Bind("DietaryProfile:ActivityMultiplier", activityMultiplier);
            var dietaryGoalCalories = new Dictionary<string, short>();
            configuration.Bind("DietaryProfile:DietaryGoalCalories", dietaryGoalCalories);
            services.AddSingleton<IDailyIntakeHelper>(new DailyIntakeHelper(activityMultiplier, dietaryGoalCalories));

            return services;
        }
    }
}