using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IBTS2026.Infrastructure.Persistence;

public class IBTS2026ContextFactory : IDesignTimeDbContextFactory<IBTS2026Context>
{
    public IBTS2026Context CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<IBTS2026Context>();

        // Use a placeholder connection string for design-time operations
        optionsBuilder.UseSqlServer("Server=.;Database=IBTS2026;Trusted_Connection=True;TrustServerCertificate=True;");

        return new IBTS2026Context(optionsBuilder.Options);
    }
}
