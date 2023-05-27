using Microsoft.EntityFrameworkCore;
using ToDoAssistant.Application.Entities;

namespace ToDoAssistant.Persistence;

public class ToDoAssistantContext : DbContext
{
    public ToDoAssistantContext(DbContextOptions<ToDoAssistantContext> options) : base(options)
    {
    }

    internal DbSet<ToDoList> Lists { get; set; }
    internal DbSet<ToDoTask> Tasks { get; set; }
    internal DbSet<ListShare> ListShares { get; set; }
    internal DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ToDoList>(x =>
        {
            x.ToTable("lists", schema: "todo");

            x.Property(e => e.UserId).IsRequired();
            x.Property(e => e.Name).IsRequired();
            x.Property(e => e.Icon).HasDefaultValue("Regular");

            x.Ignore(e => e.IsShared);
        });
        modelBuilder.Entity<ToDoTask>(x =>
        {
            x.ToTable("tasks", schema: "todo");

            x.Property(e => e.ListId).IsRequired();
            x.Property(e => e.Name).IsRequired();
        });
        modelBuilder.Entity<ListShare>(x =>
        {
            x.ToTable("shares", schema: "todo");

            x.HasKey(e => new { e.ListId, e.UserId });
        });
        modelBuilder.Entity<Notification>(x =>
        {
            x.ToTable("notifications", schema: "todo");
        });
    }
}
