using Microsoft.EntityFrameworkCore;
using NewSmartWarehouseProject.Backend.Models;

namespace NewSmartWarehouseProject.Backend.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<SensorPayload> SensorPayloads { get; set; }
    public DbSet<Alert> Alerts { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure SensorPayload entity
        modelBuilder.Entity<SensorPayload>(entity =>
        {
            entity.HasKey(e => e.Id); // Changed primary key to Id
            // You might want to configure other properties here, e.g., required fields, max lengths
        });

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique(); // Ensure email is unique
            entity.Property(e => e.Username).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
        });
    }
}