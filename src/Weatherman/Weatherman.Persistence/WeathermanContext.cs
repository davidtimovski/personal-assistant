using Microsoft.EntityFrameworkCore;
using Weatherman.Application.Entities;

namespace Core.Persistence;

public class WeathermanContext : DbContext
{
    private const string schema = "weatherman";

    public WeathermanContext(DbContextOptions<WeathermanContext> options) : base(options)
    {
    }

    internal DbSet<Forecast> Forecasts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Forecast>(x =>
        {
            x.ToTable("forecasts", schema);
            x.Property(b => b.Data).HasColumnType("json");
        });
    }
}
