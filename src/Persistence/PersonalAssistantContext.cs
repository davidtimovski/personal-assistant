using Microsoft.EntityFrameworkCore;
using Domain.Entities.Accountant;
using Domain.Entities.Common;
using Domain.Entities.CookingAssistant;
using Domain.Entities.ToDoAssistant;

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
    public DbSet<RecipeShare> RecipeShares { get; set; }
    public DbSet<SendRequest> SendRequests { get; set; }
    public DbSet<DietaryProfile> DietaryProfiles { get; set; }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<UpcomingExpense> UpcomingExpenses { get; set; }
    public DbSet<Debt> Debts { get; set; }

    public DbSet<PushSubscription> PushSubscriptions { get; set; }
    public DbSet<TooltipDismissed> TooltipsDismissed { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ToDoList>(x =>
        {
            x.ToTable("ToDoAssistant.Lists");
            x.Ignore(e => e.IsShared);
        });
        modelBuilder.Entity<ToDoTask>(x => { x.ToTable("ToDoAssistant.Tasks"); });
        modelBuilder.Entity<ListShare>(x =>
        {
            x.ToTable("ToDoAssistant.Shares");
            x.HasKey(e => new {e.ListId, e.UserId});
        });
        modelBuilder.Entity<Notification>(x => { x.ToTable("ToDoAssistant.Notifications"); });

        modelBuilder.Entity<Recipe>(x =>
        {
            x.ToTable("CookingAssistant.Recipes");
            x.Ignore(e => e.IngredientsMissing);
        });
        modelBuilder.Entity<Ingredient>(x =>
        {
            x.ToTable("CookingAssistant.Ingredients");
            x.Ignore(e => e.Recipes);
            x.Ignore(e => e.Task);
        });
        modelBuilder.Entity<RecipeIngredient>(x =>
        {
            x.ToTable("CookingAssistant.RecipesIngredients");
            x.HasKey(e => new {e.RecipeId, e.IngredientId});
        });
        modelBuilder.Entity<RecipeShare>(x =>
        {
            x.ToTable("CookingAssistant.Shares");
            x.HasKey(e => new {e.RecipeId, e.UserId});
        });
        modelBuilder.Entity<SendRequest>(x =>
        {
            x.ToTable("CookingAssistant.SendRequests");
            x.HasKey(e => new {e.RecipeId, e.UserId});
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

        modelBuilder.Entity<TooltipDismissed>(x =>
        {
            x.ToTable("TooltipsDismissed");
            x.HasKey(e => new {e.TooltipId, e.UserId});
        });
    }
}