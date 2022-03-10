using Domain.Entities.Accountant;
using Domain.Entities.Common;
using Domain.Entities.CookingAssistant;
using Domain.Entities.ToDoAssistant;
using Microsoft.EntityFrameworkCore;

namespace Persistence;

public class PersonalAssistantContext : DbContext
{
    public PersonalAssistantContext(DbContextOptions<PersonalAssistantContext> options) : base(options)
    {
    }

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
    public DbSet<DeletedEntity> DeletedEntities { get; set; }

    public DbSet<PushSubscription> PushSubscriptions { get; set; }
    public DbSet<TooltipDismissed> TooltipsDismissed { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ToDoList>(x =>
        {
            x.ToTable("ToDoAssistant.Lists");

            x.Property(e => e.Name).IsRequired();
            x.Property(e => e.Icon).HasDefaultValue("Regular");
            x.Property(e => e.NotificationsEnabled).HasDefaultValue(true);
            x.Ignore(e => e.IsShared);
        });
        modelBuilder.Entity<ToDoTask>(x => { 
            x.ToTable("ToDoAssistant.Tasks");

            x.Property(e => e.Name).IsRequired();
        });
        modelBuilder.Entity<ListShare>(x =>
        {
            x.ToTable("ToDoAssistant.Shares");
            x.HasKey(e => new { e.ListId, e.UserId });
            x.Property(e => e.NotificationsEnabled).HasDefaultValue(true);
        });
        modelBuilder.Entity<Notification>(x => { x.ToTable("ToDoAssistant.Notifications"); });

        modelBuilder.Entity<Recipe>(x =>
        {
            x.ToTable("CookingAssistant.Recipes");

            x.Property(e => e.Name).IsRequired();
            x.Ignore(e => e.IngredientsMissing);
        });
        modelBuilder.Entity<Ingredient>(x =>
        {
            x.ToTable("CookingAssistant.Ingredients");

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
            x.ToTable("CookingAssistant.IngredientBrands");
            x.Property(e => e.Name).IsRequired();
        });
        modelBuilder.Entity<RecipeIngredient>(x =>
        {
            x.ToTable("CookingAssistant.RecipesIngredients");

            x.HasKey(e => new { e.RecipeId, e.IngredientId });
            x.Property(e => e.Unit).HasMaxLength(5);
        });
        modelBuilder.Entity<IngredientTask>(x =>
        {
            x.ToTable("CookingAssistant.IngredientsTasks");
            x.HasKey(e => new { e.IngredientId, e.UserId });
        });
        modelBuilder.Entity<RecipeShare>(x =>
        {
            x.ToTable("CookingAssistant.Shares");
            x.HasKey(e => new { e.RecipeId, e.UserId });
        });
        modelBuilder.Entity<SendRequest>(x =>
        {
            x.ToTable("CookingAssistant.SendRequests");
            x.HasKey(e => new { e.RecipeId, e.UserId });
        });
        modelBuilder.Entity<DietaryProfile>(x =>
        {
            x.ToTable("CookingAssistant.DietaryProfiles");
            x.HasKey(e => e.UserId);
        });

        modelBuilder.Entity<Account>(x => { x.ToTable("Accountant.Accounts"); });
        modelBuilder.Entity<Transaction>(x => { x.ToTable("Accountant.Transactions"); });
        modelBuilder.Entity<Category>(x => { x.ToTable("Accountant.Categories"); });
        modelBuilder.Entity<UpcomingExpense>(x => { x.ToTable("Accountant.UpcomingExpenses"); });
        modelBuilder.Entity<Debt>(x => { x.ToTable("Accountant.Debts"); });
        modelBuilder.Entity<DeletedEntity>(x =>
        { 
            x.ToTable("Accountant.DeletedEntities");
            x.HasKey(e => new { e.UserId, e.EntityType, e.EntityId });
        });

        modelBuilder.Entity<TooltipDismissed>(x =>
        {
            x.ToTable("TooltipsDismissed");
            x.HasKey(e => new { e.TooltipId, e.UserId });
        });
    }
}
