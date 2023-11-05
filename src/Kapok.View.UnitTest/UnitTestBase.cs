using Kapok.Data;
using Kapok.Data.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Kapok.View.UnitTest;

public class ViewDomainUnitTestBase
{
    private IServiceProvider? _serviceProvider;

    public IServiceProvider ServiceProvider => _serviceProvider ??= BuildServiceProvider();

    public UnitTestViewDomain ViewDomain => (UnitTestViewDomain)ServiceProvider.GetRequiredService<IViewDomain>();
    public IDataDomain DataDomain => ServiceProvider.GetRequiredService<IDataDomain>();

    protected virtual void ConfigureServices(IServiceCollection serviceCollection)
    {
    }

    private IServiceProvider BuildServiceProvider()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IViewDomain, UnitTestViewDomain>();
        serviceCollection.AddSingleton<IDataDomain, InMemoryDataDomain>();

        serviceCollection.TryAdd(ServiceDescriptor.Scoped<IDataDomainScope>(p => new InMemoryDataDomainScope(p.GetRequiredService<IDataDomain>(), p)));
        serviceCollection.TryAdd(ServiceDescriptor.Scoped(typeof(IRepository<>), typeof(InMemoryRepository<>)));

        ConfigureServices(serviceCollection);

        return serviceCollection.BuildServiceProvider();
    }
}