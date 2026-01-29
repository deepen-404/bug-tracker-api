using BugTrackerApi.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerApi.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Bug> Bugs { get; set; }
    public DbSet<BugAttachment> BugAttachments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityRole>().HasData(
            new IdentityRole
            {
                Id = "1",
                Name = "User",
                NormalizedName = "USER"
            },
            new IdentityRole
            {
                Id = "2",
                Name = "Developer",
                NormalizedName = "DEVELOPER"
            }
        );
    }
}
