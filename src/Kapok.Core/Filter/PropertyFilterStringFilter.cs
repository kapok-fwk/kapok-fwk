using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using Kapok.Core.FilterParsing;
using Kapok.Entity;

namespace Kapok.Core;

public class PropertyFilterStringFilter : PropertyFilter, IPropertyFilterStringFilter
{
    public PropertyFilterStringFilter(Type baseType, string propertyName)
        : base(baseType, propertyName)
    {
    }

    public PropertyFilterStringFilter(Type baseType, PropertyInfo propertyInfo)
        : base(baseType, propertyInfo)
    {
    }

    private string? _filterString;

    public string? FilterString
    {
        get => _filterString;
        set => SetValidateProperty(ref _filterString, value);
    }

    protected override void ValidatePropertyInternal(object? value, string? propertyName, ICollection<BusinessLayerMessage> validationErrors)
    {
        if (propertyName == nameof(FilterString))
        {
            BuildFilterExpression((string?)value, out ParseException? parseException);
            if (parseException != null)
            {
                validationErrors.Add(new BusinessLayerMessage(parseException.Message, MessageSeverity.Error));
            }
        }
    }

    private void BuildFilterExpression(string? expression, out ParseException? parseException)
    {
        // don't use an actual filter on nested data filtering
        if (Attribute.IsDefined(PropertyInfo, typeof(NestedDataFilterAttribute)))
        {
            FilterExpression = null;
            parseException = null;
            return;
        }

        // it does not make sense to filter on properties which are not in the database
        // TODO filtering on calculated members will be disabled with this exception
        if (Attribute.IsDefined(PropertyInfo, typeof(NotMappedAttribute)))
        {
            FilterExpression = null;
            parseException = null;
            return;
        }
            
        if (string.IsNullOrWhiteSpace(expression))
        {
            FilterExpression = null;
            parseException = null;
            return;
        }

        var parser = new FilterExpressionParser(BaseType, PropertyInfo.Name, expression,
            Thread.CurrentThread.CurrentUICulture // TODO: should be taken from the view domain
        );

        if (parser.TryParse(out var filterExpression, out parseException))
        {
            FilterExpression = filterExpression;
        }
    }

    public override void Clear()
    {
        FilterString = null;
    }
}

public class PropertyFilterStringFilter<T> : PropertyFilter<T>, IPropertyFilterStringFilter<T>
    where T : class
{
    public PropertyFilterStringFilter(string propertyName) : base(propertyName)
    {
    }

    public PropertyFilterStringFilter(PropertyInfo propertyInfo) : base(propertyInfo)
    {
    }

    private string? _filterString;

    public string? FilterString
    {
        get => _filterString;
        set => SetValidateProperty(ref _filterString, value);
    }

    protected override void ValidatePropertyInternal(object? value, string? propertyName, ICollection<BusinessLayerMessage> validationErrors)
    {
        if (propertyName == nameof(FilterString))
        {
            BuildFilterExpression((string)value, out ParseException? parseException);
            if (parseException != null)
            {
                validationErrors.Add(new BusinessLayerMessage(parseException.Message, MessageSeverity.Error));
            }
        }
    }

    private void BuildFilterExpression(string expression, out ParseException? parseException)
    {
        // don't use an actual filter on nested data filtering
        if (Attribute.IsDefined(PropertyInfo, typeof(NestedDataFilterAttribute)))
        {
            FilterExpression = null;
            parseException = null;
            return;
        }

        // it does not make sense to filter on properties which are not in the database
        // TODO filtering on calculated members will be disabled with this exception
        if (Attribute.IsDefined(PropertyInfo, typeof(NotMappedAttribute)))
        {
            FilterExpression = null;
            parseException = null;
            return;
        }

        if (string.IsNullOrWhiteSpace(expression))
        {
            FilterExpression = null;
            parseException = null;
            return;
        }

        var parser = new FilterExpressionParser(typeof(T), PropertyInfo.Name, expression,
            Thread.CurrentThread.CurrentUICulture // TODO: should be taken from the view domain
        );

        parser.TryParse(out var filterExpression, out parseException);
        FilterExpression = (Expression<Func<T, bool>>)filterExpression;
    }

    public override void Clear()
    {
        FilterString = null;
    }
}