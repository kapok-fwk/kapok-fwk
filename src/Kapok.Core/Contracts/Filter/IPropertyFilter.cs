using System.Reflection;

namespace Kapok.BusinessLayer;

public interface IPropertyFilter : IFilter
{
    Type BaseType { get; }

    PropertyInfo PropertyInfo { get; }
}

public interface IPropertyFilter<T> : IFilter<T>, IPropertyFilter
{
}