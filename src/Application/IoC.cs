using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PersonalAssistant.Application.Contracts;
using PersonalAssistant.Application.Contracts.Accountant.Accounts;
using PersonalAssistant.Application.Contracts.Accountant.Categories;
using PersonalAssistant.Application.Contracts.Accountant.Common;
using PersonalAssistant.Application.Contracts.Accountant.Debts;
using PersonalAssistant.Application.Contracts.Accountant.Transactions;
using PersonalAssistant.Application.Contracts.Accountant.UpcomingExpenses;
using PersonalAssistant.Application.Contracts.Common;
using PersonalAssistant.Application.Contracts.Common.Models;
using PersonalAssistant.Application.Contracts.CookingAssistant;
using PersonalAssistant.Application.Contracts.CookingAssistant.Common;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles;
using PersonalAssistant.Application.Contracts.CookingAssistant.DietaryProfiles.Models;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients;
using PersonalAssistant.Application.Contracts.CookingAssistant.Ingredients.Models;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes;
using PersonalAssistant.Application.Contracts.CookingAssistant.Recipes.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Lists.Models;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Notifications;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks;
using PersonalAssistant.Application.Contracts.ToDoAssistant.Tasks.Models;
using PersonalAssistant.Application.Services;
using PersonalAssistant.Application.Services.Accountant;
using PersonalAssistant.Application.Services.Common;
using PersonalAssistant.Application.Services.CookingAssistant;
using PersonalAssistant.Application.Services.ToDoAssistant;
using Utility;

namespace PersonalAssistant.Application
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