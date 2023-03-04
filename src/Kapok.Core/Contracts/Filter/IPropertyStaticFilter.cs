namespace Kapok.BusinessLayer;

public interface IPropertyStaticFilter : IPropertyFilter
{
    object? FilterValue { get; set; }
}

public interface IPropertyStaticFilter<T> : IPropertyFilter<T>, IPropertyStaticFilter
    where T : class
{
}