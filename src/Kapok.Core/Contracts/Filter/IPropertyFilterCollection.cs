namespace Kapok.Core;

public interface IPropertyFilterCollection : IFilter
{
    ICollection<IPropertyFilter> Properties { get; }

    // NOTE: I had to implement this because there is no replace implementation in ICollection<T>
    void ReplacePropertyFilter(IPropertyFilter oldFilter, IPropertyFilter newFilter);
}

public interface IPropertyFilterCollection<T> : IPropertyFilterCollection, IFilter<T>
{
}