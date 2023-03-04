using System.ComponentModel;

namespace Kapok.Entity;

/// <summary>
/// This is a helper class when working with classes implementing INotifyPropertyChanging and
/// INotifyPropertyChanged.
/// 
/// You might need to access the old value of a property when the event from 'INotifyPropertyChanged'
/// is called. This helper class stores the last value of a property when calling
/// this.OnPropertyChanging and returns its value when calling this.GetLastPropertyValue.
///
/// The cache is automatically cleared when this.OnPropertyChanging is called from another property.
/// This class should therefore be instantiated per listener (e.g. an class instance, an class instance list etc.).
/// It should not be used to cache property values over different classes.
/// </summary>
public class NotifyPropertyChangingCache
{
    public NotifyPropertyChangingCache()
    {
        _senderType = null;
    }

    public NotifyPropertyChangingCache(Type? senderType)
    {
        _senderType = senderType;
    }

    private readonly Type? _senderType;

    private object? _lastSender;
    private readonly Dictionary<string, object?> _propertyOldValueCache = new();

    private object? GetPropertyValue(object sender, string propertyName)
    {
        var senderType = _senderType;
        if (senderType == null)
            senderType = sender.GetType();

        var propertyInfo = senderType.GetProperty(propertyName);

        if (propertyInfo == null)
            return null; // Could not find property! --> optimistic behavior here, we ignore this

        if (propertyInfo.GetMethod == null)
            return null; // The property does not offer a 'Get' method --> optimistic behavior here, we ignore this

        return propertyInfo.GetMethod.Invoke(sender, Array.Empty<object>());
    }

    public void OnPropertyChanging(object? sender, string? propertyName)
    {
        if (sender == null || propertyName == null)
            return;

        if (_lastSender != sender)
        {
            _propertyOldValueCache.Clear();
            _lastSender = sender;
        }

        var currentPropertyValue = GetPropertyValue(sender, propertyName);

        if (_propertyOldValueCache.ContainsKey(propertyName))
        {
            _propertyOldValueCache[propertyName] = currentPropertyValue;
        }
        else
        {
            _propertyOldValueCache.Add(propertyName, currentPropertyValue);
        }
    }

    public void OnPropertyChanging(object sender, PropertyChangingEventArgs eventArgs)
    {
        OnPropertyChanging(sender, eventArgs.PropertyName);
    }

    public object? GetLastPropertyValue(object sender, string propertyName)
    {
        if (sender != _lastSender)
            return null; // sender entity different --> optimistic behavior here, we ignore this

        if (!_propertyOldValueCache.ContainsKey(propertyName))
            return null; // 'PropertyChanging' not registered before call of this method --> optimistic behavior here, we ignore this

        return _propertyOldValueCache[propertyName];
    }

    /// <summary>
    /// Clears the cache.
    /// </summary>
    public void Clear()
    {
        _lastSender = null;
        _propertyOldValueCache.Clear();
    }
}