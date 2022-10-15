using Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Account.Models;

public class AccountContext : DbContext
{
    public AccountContext(DbContextOptions<AccountContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().Property(u => u.Name).IsRequired().HasMaxLength(30);
        builder.Entity<ApplicationUser>().Property(u => u.Language).IsRequired().HasMaxLength(5);
    }
}
