using Microsoft.EntityFrameworkCore;
using Angelus.Domain.Entities;

namespace Angelus.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Character> Characters => Set<Character>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.Property(u => u.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Character>(e =>
        {
            e.HasKey(c => c.Id);
            e.HasIndex(c => c.Name).IsUnique();
            e.Property(c => c.Name).IsRequired().HasMaxLength(20);
            e.Property(c => c.AngelType).IsRequired().HasMaxLength(10);
            e.HasOne(c => c.User).WithMany(u => u.Characters).HasForeignKey(c => c.UserId);
        });
    }
}
