using Microsoft.EntityFrameworkCore;
using USERSERVICE.Models;
using USERSERVICE.Models.Sensor;

namespace USERSERVICE.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Alert> Alerts { get; set; }
}