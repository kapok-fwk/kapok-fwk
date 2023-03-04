namespace Kapok.BusinessLayer;

public interface IPropertyFilterStringFilter : IPropertyFilter
{
    string? FilterString { get; set; }
}

public interface IPropertyFilterStringFilter<T> : IPropertyFilter<T>, IPropertyFilterStringFilter
{
}