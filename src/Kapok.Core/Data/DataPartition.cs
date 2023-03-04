using System.Reflection;

namespace Kapok.Data;

public class DataPartition
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="interfaceType"></param>
    /// <param name="partitionProperty">
    /// The partition property must be a type which cannot be <c>null</c>.
    /// </param>
    public DataPartition(Type interfaceType, PropertyInfo partitionProperty)
    {
        InterfaceType = interfaceType;
        PartitionProperty = partitionProperty;
        Value = null;
    }

    /// <summary>
    /// Interface type which an entity must include to be part
    /// of the data partition.
    /// </summary>
    public Type InterfaceType { get; }

    /// <summary>
    /// The property identifying the data partition.
    /// </summary>
    public PropertyInfo PartitionProperty { get; }

    /// <summary>
    /// The current value of the data scope.
    /// </summary>
    public object? Value { get; set; }
}