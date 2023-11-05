using Kapok.Data;
using Kapok.Data.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Kapok.View.UnitTest;

public class ViewDomainUnitTestBase
{
    public IServiceProvider ServiceProvider { get; }

    public UnitTestViewDomain ViewDomain => (UnitTestViewDomain)ServiceProvider.GetRequiredService<IViewDomain>();
    public IDataDomain DataDomain => ServiceProvider.GetRequiredService<IDataDomain>();

    public ViewDomainUnitTestBase()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IViewDomain, UnitTestViewDomain>();
        serviceCollection.AddSingleton<IDataDomain, InMemoryDataDomain>();

        serviceCollection.TryAdd(ServiceDescriptor.Scoped<IDataDomainScope>(p => new InMemoryDataDomainScope(p.GetRequiredService<IDataDomain>(), p)));
        serviceCollection.TryAdd(ServiceDescriptor.Scoped(typeof(IRepository<>), typeof(InMemoryRepository<>)));

        ServiceProvider = serviceCollection.BuildServiceProvider();
    }
}