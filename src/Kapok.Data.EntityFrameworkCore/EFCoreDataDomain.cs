using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kapok.Data.EntityFrameworkCore;

public class EFCoreDataDomain : DataDomain, IEntityFrameworkCoreDataDomain
{
    public EFCoreDataDomain(DbContextOptions dbContextOptions)
    {
        DbContextOptions = dbContextOptions;
    }

    protected override void ConfigureServices(IServiceCollection serviceCollection)
    {
        base.ConfigureServices(serviceCollection);
        serviceCollection.AddScoped<IDataDomainScope>(p =>
            new EFCoreDataDomainScope(p.GetRequiredService<IDataDomain>(), p));
        serviceCollection.TryAdd(ServiceDescriptor.Scoped(typeof(IRepository<>), typeof(EFCoreRepository<>)));
    }

    public override IDataDomainScope CreateScope()
    {
        var scope = ServiceProvider.CreateScope();
        return scope.ServiceProvider.GetRequiredService<IDataDomainScope>();
    }

    public DbContextOptions DbContextOptions { get; }

    public DbContext ConstructNewDbContext()
    {
        return new DbContextBase(DbContextOptions);
    }
}