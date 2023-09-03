using System.Collections;

namespace Kapok.Data.InMemory;

/// <summary>
/// A in memory data domain which holds the data until the DataDomain is disposed.  
/// </summary>
public class InMemoryDataDomain : DataDomain
{
    // holds the in memory cache of the entities.
    internal Dictionary<Type, IList> InMemoryData = new();

    public override IDataDomainScope CreateScope()
    {
        return new InMemoryDataDomainScope(this);
    }

    internal List<T> GetInMemoryData<T>()
    {
        var type = typeof(T);

        if (!DataEntities.Contains(type))
            throw new ArgumentException(
                "You can only pass a entity type which has already been registered to the DataDomain");

        if (InMemoryData.TryGetValue(type, out var inMemoryList))
        {
            return (List<T>)inMemoryList;
        }

        var newList = new List<T>();
        InMemoryData.Add(type, newList);
        return newList;
    }
}