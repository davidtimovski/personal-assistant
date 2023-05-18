using CookingAssistant.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace CookingAssistant.Persistence;

public class CookingAssistantContext : DbContext
{
    public CookingAssistantContext(DbContextOptions<CookingAssistantContext> options) : base(options)
    {
    }

    internal DbSet<Recipe> Recipes { get; set; }
    internal DbSet<Ingredient> Ingredients { get; set; }
    internal DbSet<RecipeIngredient> RecipesIngredients { get; set; }
    internal DbSet<IngredientTask> IngredientsTasks { get; set; }
    internal DbSet<RecipeShare> RecipeShares { get; set; }
    internal DbSet<SendRequest> SendRequests { get; set; }
    internal DbSet<DietaryProfile> DietaryProfiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>(x =>
        {
            x.ToTable("recipes", schema: "cooking");

            x.Property(e => e.Name).IsRequired();
            x.Ignore(e => e.IngredientsMissing);
        });
        modelBuilder.Entity<Ingredient>(x =>
        {
            x.ToTable("ingredients", schema: "cooking");

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
            x.ToTable("ingredient_brands", schema: "cooking");
            x.Property(e => e.Name).IsRequired();
        });
        modelBuilder.Entity<RecipeIngredient>(x =>
        {
            x.ToTable("recipes_ingredients", schema: "cooking");

            x.HasKey(e => new { e.RecipeId, e.IngredientId });
            x.Property(e => e.Unit).HasMaxLength(5);
        });
        modelBuilder.Entity<IngredientTask>(x =>
        {
            x.ToTable("ingredients_tasks", schema: "cooking");
            x.HasKey(e => new { e.IngredientId, e.UserId });
        });
        modelBuilder.Entity<RecipeShare>(x =>
        {
            x.ToTable("shares", schema: "cooking");
            x.HasKey(e => new { e.RecipeId, e.UserId });
        });
        modelBuilder.Entity<SendRequest>(x =>
        {
            x.ToTable("send_requests", schema: "cooking");
            x.HasKey(e => new { e.RecipeId, e.UserId });
        });
        modelBuilder.Entity<DietaryProfile>(x =>
        {
            x.ToTable("dietary_profiles", schema: "cooking");
            x.HasKey(e => e.UserId);
        });
    }
}
