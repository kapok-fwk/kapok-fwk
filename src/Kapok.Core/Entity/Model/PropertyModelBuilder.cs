using System.Linq.Expressions;
using System.Reflection;
using Kapok.BusinessLayer;
using Kapok.Data;

namespace Kapok.Entity.Model;

public sealed class PropertyModelBuilder<T>
    where T : class
{
    private readonly EntityProperty _propertyModel;

    internal PropertyModelBuilder(EntityProperty propertyModel)
    {
        _propertyModel = propertyModel;
    }

    public PropertyModelBuilder<T> AddLookup<TLookupEntry>(Func<IDataDomainScope, IQueryable<TLookupEntry>> lookup)
        where TLookupEntry : class
    {
        if (_propertyModel.LookupDefinition != null)
            throw new NotSupportedException($"The method {nameof(AddLookup)} can not be configured twice for the property {_propertyModel.PropertyName}");

        var lookupDefinition = new LookupDefinition<T, TLookupEntry, object>(lookup);

        _propertyModel.LookupDefinition = lookupDefinition;
            
        return this;
    }

    public PropertyModelBuilder<T> AddLookup<TLookupEntry>(Func<T, IDataDomainScope, IQueryable<TLookupEntry>> lookup)
        where TLookupEntry : class
    {
        if (_propertyModel.LookupDefinition != null)
            throw new NotSupportedException($"The method {nameof(AddLookup)} can not be configured twice for the property {_propertyModel.PropertyName}");

        var lookupDefinition = new LookupDefinition<T, TLookupEntry, object>(lookup);

        _propertyModel.LookupDefinition = lookupDefinition;
            
        return this;
    }

    public PropertyModelBuilder<T> AddLookup<TLookupEntry, TFieldType>(Func<IDataDomainScope, IQueryable<TLookupEntry>> lookup,
        Expression<Func<TLookupEntry, TFieldType>> fieldSelector)
        where TLookupEntry : class
    {
        if (_propertyModel.LookupDefinition != null)
            throw new NotSupportedException($"The method {nameof(AddLookup)} can not be configured twice for the property {_propertyModel.PropertyName}");

        var lookupDefinition = new LookupDefinition<T, TLookupEntry, TFieldType>(lookup, fieldSelector);

        _propertyModel.LookupDefinition = lookupDefinition;
            
        return this;
    }

    public PropertyModelBuilder<T> AddLookup<TLookupEntry, TFieldType>(Func<T, IDataDomainScope, IQueryable<TLookupEntry>> lookup,
        Expression<Func<TLookupEntry, TFieldType>> fieldSelector)
        where TLookupEntry : class
    {
        if (_propertyModel.LookupDefinition != null)
            throw new NotSupportedException($"The method {nameof(AddLookup)} can not be configured twice for the property {_propertyModel.PropertyName}");

        var lookupDefinition = new LookupDefinition<T, TLookupEntry, TFieldType>(lookup, fieldSelector);

        _propertyModel.LookupDefinition = lookupDefinition;
            
        return this;
    }

    public PropertyModelBuilder<T> AddCalculation<TFieldType>(Expression<Func<T, TFieldType>> calculateFunc)
    {
        if (_propertyModel.CalculateDefinition != null)
            throw new NotSupportedException($"The method {nameof(AddCalculation)} can not be configured twice for the property {_propertyModel.PropertyName}");

        var calculationDefinition = new PropertyCalculateDefinition<T, TFieldType>(calculateFunc);

        _propertyModel.CalculateDefinition = calculationDefinition;

        return this;
    }

    public PropertyModelBuilder<T> AddCalculation(IPropertyCalculateDefinition propertyCalculateDefinition)
    {
        if (_propertyModel.CalculateDefinition != null)
            throw new NotSupportedException($"The method {nameof(AddCalculation)} can not be configured twice for the property {_propertyModel.PropertyName}");

        _propertyModel.CalculateDefinition = propertyCalculateDefinition;

        return this;
    }

    public PropertyModelBuilder<T> AddCalculation<TFieldType>(MethodInfo calculateFuncBuilderMethod)
    {
        if (_propertyModel.CalculateDefinition != null)
            throw new NotSupportedException($"The method {nameof(AddCalculation)} can not be configured twice for the property {_propertyModel.PropertyName}");

        _propertyModel.CalculateDefinition = new PropertyCalculateDefinition<T, TFieldType>(calculateFuncBuilderMethod);

        return this;
    }

    public PropertyModelBuilder<T> AddDrillDown<TDestinationEntry>(Type pageType, Action<IFilterSet<TDestinationEntry>, T, IReadOnlyDictionary<string, object?>> filter)
        where TDestinationEntry : class
    {
        if (_propertyModel.DrillDownDefinition != null)
            throw new NotSupportedException($"The method {nameof(AddDrillDown)} can not be configured twice for the property {_propertyModel.PropertyName}");

        var drillDownDefinition = new DrillDownDefinition<TDestinationEntry, T>(pageType, filter);

        _propertyModel.DrillDownDefinition = drillDownDefinition;

        return this;
    }
}