using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using NewSmartWarehouseProject.Backend.Data;

namespace NewSmartWarehouseProject.Backend;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        // Use a dummy connection string for design-time, as it's not used for actual database operations during migration creation.
        // The real connection string will be used when applying migrations.
        optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DesignDb;Trusted_Connection=True;MultipleActiveResultSets=true");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

