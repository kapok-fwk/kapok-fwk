namespace Kapok.Data;

public interface IDataDomain
{
    /// <summary>
    /// Adds a data partition to the data domain.
    /// 
    /// A data partition can be a tenant id or a company in financial terms.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="interfaceType"></param>
    /// <param name="propertyName"></param>
    void RegisterDataPartition(string name, Type interfaceType, string propertyName);

    /// <summary>
    /// Gets or sets the value of the data scope.
    /// </summary>
    IReadOnlyDictionary<string, DataPartition> DataPartitions { get; }

    IDataDomainScope CreateScope();
}