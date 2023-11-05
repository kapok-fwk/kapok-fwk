using System.Collections;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kapok.Data.InMemory;

/// <summary>
/// A in memory data domain which holds the data until the DataDomain is disposed.  
/// </summary>
public class InMemoryDataDomain : DataDomain
{
    // holds the in memory cache of the entities.
    private readonly Dictionary<Type, IList> _inMemoryData = new();

    protected override void ConfigureServices(IServiceCollection serviceCollection)
    {
        base.ConfigureServices(serviceCollection);
        serviceCollection.TryAdd(ServiceDescriptor.Scoped<IDataDomainScope>(p => new InMemoryDataDomainScope(p.GetRequiredService<IDataDomain>(), p)));
        serviceCollection.TryAdd(ServiceDescriptor.Scoped(typeof(IRepository<>), typeof(InMemoryRepository<>)));
    }

    public override IDataDomainScope CreateScope()
    {
        var scope = ServiceProvider.CreateScope();
        return new InMemoryDataDomainScope(this, scope.ServiceProvider);
    }

    internal List<T> GetInMemoryData<T>()
    {
        var type = typeof(T);

        if (!DataEntities.Contains(type))
            throw new ArgumentException(
                "You can only pass a entity type which has already been registered to the DataDomain");

        if (_inMemoryData.TryGetValue(type, out var inMemoryList))
        {
            return (List<T>)inMemoryList;
        }

        var newList = new List<T>();
        _inMemoryData.Add(type, newList);
        return newList;
    }
}