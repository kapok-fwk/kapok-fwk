using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kapok.Data.EntityFrameworkCore.UnitTest;

public abstract class EFUnitTestBase
{
    private IServiceProvider? _serviceProvider;

    public IServiceProvider ServiceProvider => _serviceProvider ??= BuildServiceProvider();

    public IDataDomain DataDomain => ServiceProvider.GetRequiredService<IDataDomain>();

    protected virtual void ConfigureServices(IServiceCollection serviceCollection)
    {
    }

    private IServiceProvider BuildServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IDataDomain, EFCoreDataDomain>(p => (EFCoreDataDomain)InitializeDataDomain());

        serviceCollection.TryAdd(ServiceDescriptor.Scoped<IDataDomainScope>(p => new EFCoreDataDomainScope(p.GetRequiredService<IDataDomain>(), p)));
        serviceCollection.TryAdd(ServiceDescriptor.Scoped(typeof(IRepository<>), typeof(EFCoreRepository<>)));

        ConfigureServices(serviceCollection);

        return serviceCollection.BuildServiceProvider();
    }

    public IDataDomain InitializeDataDomain(DbContextOptions? dbContextOptions = null)
    {
        if (dbContextOptions == null)
        {
            var contextOptionBuilder = new DbContextOptionsBuilder();

            // testing against SQLite database
            // Requires NuGet package Microsoft.EntityFrameworkCore.Sqlite
            contextOptionBuilder.UseSqlite($"Data Source={GetType().FullName};Mode=Memory;Cache=Shared");

            // testing against a local SQL Server database
            // Requires NuGet package Microsoft.EntityFrameworkCore.SqlServer
            //contextOptionBuilder.UseSqlServer(@"Server=(local);Database=EFProviders.InMemory;Trusted_Connection=True;ConnectRetryCount=0",
            //    opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(10).TotalSeconds));

            dbContextOptions = contextOptionBuilder.Options;
        }

        InitiateModule();

        return new EFCoreDataDomain(dbContextOptions);
    }

    protected virtual void InitiateModule()
    {
    }
}