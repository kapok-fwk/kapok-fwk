using Kapok.Core;

namespace Kapok.Data.InMemory;

public class InMemoryDataDomainScope : DataDomainScope
{
    public InMemoryDataDomainScope(IDataDomain dataDomain) : base(dataDomain)
    {
    }

    protected override IRepository<T> InitializeRepository<T>()
    {
        return new InMemoryRepository<T>();
    }

    public override bool CanSave()
    {
        return true;
    }

    public override void Save()
    {
        // Note: We don't do anything here. The in memory data domain scope will loose
        //       all its data after disposing
    }

    public override Task SaveAsync(CancellationToken cancellationToken = default)
    {
        // Note: We don't do anything here. The in memory data domain scope will loose
        //       all its data after disposing
        return Task.CompletedTask;
    }

    public override void RejectChanges()
    {
        // Note: We don't do anything here. The in memory data domain scope will loose
        //       all its data after disposing
    }
}