using Kapok.BusinessLayer;
using Kapok.BusinessLayer.FilterParsing;

namespace Kapok.View;

public class PropertyFilterViewModel<TEntry> : ValidatableBindableObjectBase
{
    public PropertyFilterViewModel()
    {
        FilterLayer = FilterLayer.User;
    }

    private FilterLayer _filterLayer;
    private PropertyViewModel? _property;
    private bool _isReadOnly;
    private object? _value;

    public FilterLayer FilterLayer
    {
        get => _filterLayer;
        set => SetProperty(ref _filterLayer, value);
    }

    public PropertyViewModel? Property
    {
        get => _property;
        set => SetProperty(ref _property, value);
    }

    public bool IsReadOnly
    {
        get => _isReadOnly;
        set => SetProperty(ref _isReadOnly, value);
    }

    public object? Value
    {
        get => _value;
        set
        {
            if (SetValidateProperty(ref _value, value))
                ValueIsChanged = true;
        }
    }

    public bool ValueIsChanged { get; set; }

    public IPropertyFilter? PropertyFilter { get; set; }

    protected override void ValidatePropertyInternal(object? value, string? propertyName, ICollection<BusinessLayerMessage> validationErrors)
    {
        if (propertyName == nameof(Value))
        {
            if (Property != null && PropertyFilter != null && value != null)
            {
#pragma warning disable CS8604
                var parser = new FilterExpressionParser(typeof(TEntry), Property.PropertyInfo.Name, value.ToString(),
#pragma warning restore CS8604
                    Thread.CurrentThread.CurrentUICulture
                );
                if (!parser.TryParse(out _, out ParseException? parseException))
                {
                    validationErrors.Add(new BusinessLayerMessage(parseException?.Message ?? "Filter not parsable", MessageSeverity.Error));
                }
            }
        }
    }
}