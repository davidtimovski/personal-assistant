using Domain.Accountant;
using Domain.Common;
using Domain.CookingAssistant;
using Domain.ToDoAssistant;
using Domain.Weatherman;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class PersonalAssistantContext : DbContext
{
    public PersonalAssistantContext(DbContextOptions<PersonalAssistantContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<ToDoList> Lists { get; set; }
    public DbSet<ToDoTask> Tasks { get; set; }
    public DbSet<ListShare> ListShares { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<RecipeIngredient> RecipesIngredients { get; set; }
    public DbSet<IngredientTask> IngredientsTasks { get; set; }
    public DbSet<RecipeShare> RecipeShares { get; set; }
    public DbSet<SendRequest> SendRequests { get; set; }
    public DbSet<DietaryProfile> DietaryProfiles { get; set; }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<UpcomingExpense> UpcomingExpenses { get; set; }
    public DbSet<Debt> Debts { get; set; }
    public DbSet<AutomaticTransaction> AutomaticTransactions { get; set; }
    public DbSet<DeletedEntity> DeletedEntities { get; set; }

    public DbSet<Forecast> Forecasts { get; set; }

    public DbSet<PushSubscription> PushSubscriptions { get; set; }
    public DbSet<TooltipDismissed> TooltipsDismissed { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(x =>
        {
            x.Property(e => e.ToDoNotificationsEnabled).HasColumnName("todo_notifications_enabled");
        });

        modelBuilder.Entity<ToDoList>(x =>
        {
            x.ToTable("todo_lists");

            x.Property(e => e.UserId).IsRequired();
            x.Property(e => e.Name).IsRequired();
            x.Property(e => e.Icon).HasDefaultValue("Regular");
            x.Property(e => e.NotificationsEnabled).HasDefaultValue(true);

            x.Ignore(e => e.IsShared);
        });
        modelBuilder.Entity<ToDoTask>(x =>
        {
            x.ToTable("todo_tasks");

            x.Property(e => e.ListId).IsRequired();
            x.Property(e => e.Name).IsRequired();
        });
        modelBuilder.Entity<ListShare>(x =>
        {
            x.ToTable("todo_shares");

            x.HasKey(e => new { e.ListId, e.UserId });

            x.Property(e => e.NotificationsEnabled).HasDefaultValue(true);
        });
        modelBuilder.Entity<Notification>(x =>
        {
            x.ToTable("todo_notifications");
        });

        modelBuilder.Entity<Recipe>(x =>
        {
            x.ToTable("cooking_recipes");

            x.Property(e => e.Name).IsRequired();
            x.Ignore(e => e.IngredientsMissing);
        });
        modelBuilder.Entity<Ingredient>(x =>
        {
            x.ToTable("cooking_ingredients");

            x.Property(e => e.Name).IsRequired();
            x.Property(e => e.ServingSize).HasDefaultValue(100);
            x.Property(e => e.ProductSize).HasDefaultValue(100);
            x.Ignore(e => e.Recipes);
            x.Ignore(e => e.Task);
            x.Ignore(e => e.TaskId);
            x.Ignore(e => e.RecipeCount);
        });
        modelBuilder.Entity<IngredientBrand>(x =>
        {
            x.ToTable("cooking_ingredient_brands");
            x.Property(e => e.Name).IsRequired();
        });
        modelBuilder.Entity<RecipeIngredient>(x =>
        {
            x.ToTable("cooking_recipes_ingredients");

            x.HasKey(e => new { e.RecipeId, e.IngredientId });
            x.Property(e => e.Unit).HasMaxLength(5);
        });
        modelBuilder.Entity<IngredientTask>(x =>
        {
            x.ToTable("cooking_ingredients_tasks");
            x.HasKey(e => new { e.IngredientId, e.UserId });
        });
        modelBuilder.Entity<RecipeShare>(x =>
        {
            x.ToTable("cooking_shares");
            x.HasKey(e => new { e.RecipeId, e.UserId });
        });
        modelBuilder.Entity<SendRequest>(x =>
        {
            x.ToTable("cooking_send_requests");
            x.HasKey(e => new { e.RecipeId, e.UserId });
        });
        modelBuilder.Entity<DietaryProfile>(x =>
        {
            x.ToTable("cooking_dietary_profiles");
            x.HasKey(e => e.UserId);
        });

        modelBuilder.Entity<Account>(x => { x.ToTable("accountant_accounts"); });
        modelBuilder.Entity<Transaction>(x => { x.ToTable("accountant_transactions"); });
        modelBuilder.Entity<Category>(x => { x.ToTable("accountant_categories"); });
        modelBuilder.Entity<UpcomingExpense>(x => { x.ToTable("accountant_upcoming_expenses"); });
        modelBuilder.Entity<Debt>(x => { x.ToTable("accountant_debts"); });
        modelBuilder.Entity<AutomaticTransaction>(x => { x.ToTable("accountant_automatic_transactions"); });
        modelBuilder.Entity<DeletedEntity>(x =>
        {
            x.ToTable("accountant_deleted_entities");
            x.HasKey(e => new { e.UserId, e.EntityType, e.EntityId });
        });

        modelBuilder.Entity<Forecast>(x =>
        {
            x.ToTable("weatherman_forecasts");
            x.Property(b => b.Data).HasColumnType("json");
        });

        modelBuilder.Entity<Tooltip>(x => { x.ToTable("tooltips"); });
        modelBuilder.Entity<TooltipDismissed>(x =>
        {
            x.ToTable("tooltips_dismissed");
            x.HasKey(e => new { e.TooltipId, e.UserId });
        });
    }
}
