using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonalAssistant.Infrastructure.Identity;

namespace Auth.Models
{
    public class PersonalAssistantAuthContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public PersonalAssistantAuthContext(DbContextOptions<PersonalAssistantAuthContext> options)
        : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>().Property(u => u.Name).IsRequired().HasMaxLength(30);
            builder.Entity<ApplicationUser>().Property(u => u.Language).IsRequired().HasMaxLength(5);
            builder.Entity<ApplicationUser>().Property(u => u.DateRegistered).IsRequired();
        }
    }
}
