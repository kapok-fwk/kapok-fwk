using System.Linq.Expressions;
using System.Reflection;

namespace Kapok.BusinessLayer;

public abstract class PropertyFilter : ValidatableBindableObjectBase, IPropertyFilter
{
    protected PropertyFilter(Type baseType, string propertyName)
    {
        BaseType = baseType;

        var propertyInfo = baseType.GetProperty(propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

        if (propertyInfo == null)
            throw new ArgumentException($"Property with name {propertyName} not found in type {baseType.FullName}.", nameof(propertyName));

        PropertyInfo = propertyInfo;
    }

    protected PropertyFilter(Type baseType, PropertyInfo propertyInfo)
    {
        BaseType = baseType;
        PropertyInfo = propertyInfo;
    }

    public Type BaseType { get; }

    public PropertyInfo PropertyInfo { get; }

    private Expression? _filterExpression;

    public Expression? FilterExpression
    {
        get => _filterExpression;
        set
        {
            if (_filterExpression == value) return;
            _filterExpression = value;
            OnFilterChanged();
        }
    }

    public event EventHandler? FilterChanged;

    private void OnFilterChanged()
    {
        FilterChanged?.Invoke(this, EventArgs.Empty);
    }

    public abstract void Clear();
}

public abstract class PropertyFilter<T> : ValidatableBindableObjectBase, IPropertyFilter<T>
    where T : class
{
    protected PropertyFilter(string propertyName)
    {
        var propertyInfo = typeof(T).GetProperty(propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.SetProperty);

        if (propertyInfo == null)
            throw new ArgumentException($"Property with name {propertyName} not found in type {typeof(T).FullName}.", nameof(propertyName));

        PropertyInfo = propertyInfo;
    }

    protected PropertyFilter(PropertyInfo propertyInfo)
    {
        PropertyInfo = propertyInfo;
    }

    public PropertyInfo PropertyInfo { get; }

    private Expression<Func<T, bool>>? _filterExpression;

    public Expression<Func<T, bool>>? FilterExpression
    {
        get => _filterExpression;
        set
        {
            if (_filterExpression == value) return;
            _filterExpression = value;
            OnFilterChanged();
        }
    }

    public event EventHandler? FilterChanged;

    private void OnFilterChanged()
    {
        FilterChanged?.Invoke(this, EventArgs.Empty);
    }

    public abstract void Clear();

    #region IPropertyFilter

    Type IPropertyFilter.BaseType => typeof(T);

    #endregion

    #region IFilter

    Expression? IFilter.FilterExpression => FilterExpression;

    #endregion
}