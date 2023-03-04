using Microsoft.EntityFrameworkCore;

namespace Kapok.Data.EntityFrameworkCore;

public class EFCoreDataDomain : DataDomain, IEntityFrameworkCoreDataDomain
{
    public EFCoreDataDomain(DbContextOptions dbContextOptions)
    {
        DbContextOptions = dbContextOptions;
    }

    public override IDataDomainScope CreateScope()
    {
        var scope = new EFCoreDataDomainScope(this);
        return scope;
    }

    public DbContextOptions DbContextOptions { get; }

    public DbContext ConstructNewDbContext()
    {
        return new DbContextBase(DbContextOptions);
    }
}