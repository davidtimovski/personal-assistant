using Core.Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace Core.Persistence;

public class CoreContext : DbContext
{
    public CoreContext(DbContextOptions<CoreContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }

    public DbSet<PushSubscription> PushSubscriptions { get; set; }
    public DbSet<TooltipDismissed> TooltipsDismissed { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(x =>
        {
            x.Property(e => e.ToDoNotificationsEnabled).HasColumnName("todo_notifications_enabled");
        });

        modelBuilder.Entity<Tooltip>(x => x.ToTable("tooltips"));
        modelBuilder.Entity<TooltipDismissed>(x =>
        {
            x.ToTable("tooltips_dismissed");
            x.HasKey(e => new { e.TooltipId, e.UserId });
        });
    }
}
