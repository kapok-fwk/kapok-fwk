using System.Collections;
using System.ComponentModel;
using System.Reflection;

namespace Kapok.Data;

public enum ChangeTrackingState
{
    None,
    Created,
    Updated,
    Deleted,
    Detached
}

public class ChangeTracking
{
    public ChangeTracking(object entity, Type entityType, object? originalEntity = null)
    {
        EntityType = entityType;
        Entity = entity;
        OriginalEntity = originalEntity;
    }

    public readonly object Entity;
    public readonly Type EntityType;
    public object? OriginalEntity;
    public ChangeTrackingState State = ChangeTrackingState.None;

    public string? LastPropertyNameChanging;
    public object? LastPropertyValueChanging;
}

public static class DtoMapper
{
    public static PropertyInfo[] GetProperties(Type dtoType)
    {
        return dtoType.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite).ToArray();
    }

    public static object Map(Type dtoType, object dto)
    {
        var instance = Activator.CreateInstance(dtoType);
#pragma warning disable CS8604
        Map(dtoType, dto, instance);
#pragma warning restore CS8604
        return instance;
    }

    public static object Map(Type dtoType, PropertyInfo[] propertyList, object dto)
    {
        var instance = Activator.CreateInstance(dtoType);
#pragma warning disable CS8604
        Map(propertyList, dto, instance);
#pragma warning restore CS8604
        return instance;
    }

    public static void Map(Type dtoType, object fromDto, object toDto)
    {
        foreach (var p in GetProperties(dtoType))
        {
#pragma warning disable CS8602
            p.SetMethod.Invoke(
                toDto,
                new[] { p.GetMethod.Invoke(fromDto, null) }
            );
#pragma warning restore CS8602
        }
    }

    public static void Map(PropertyInfo[] propertyList, object fromDto, object toDto)
    {
        foreach (var p in propertyList)
        {
#pragma warning disable CS8602
            p.SetMethod.Invoke(
                toDto,
                new[] { p.GetMethod.Invoke(fromDto, null) }
            );
#pragma warning restore CS8602
        }
    }
}
    
public class DtoMapper<TDtoType>
    where TDtoType : class
{
    readonly PropertyInfo[] _properties;

    public DtoMapper()
    {
        // Cache property infos
        var t = typeof(TDtoType);
        _properties = DtoMapper.GetProperties(t);
    }

    public TDtoType Map(TDtoType dto)
    {
        return (TDtoType)DtoMapper.Map(typeof(TDtoType), _properties, dto);
    }
}

public class ChangeTracker : IEnumerable<ChangeTracking>
{
    private readonly List<ChangeTracking> _changeTracker = new();

    public bool AnyChangesOutstanding()
    {
        return _changeTracker.Any(
            ct => ct.State == ChangeTrackingState.Created ||
                  ct.State == ChangeTrackingState.Updated ||
                  ct.State == ChangeTrackingState.Deleted
        );
    }

    private void RegisterEvents(ChangeTracking trackingObject)
    {
        if (trackingObject.Entity is INotifyPropertyChanging entityNotifyPropertyChanging)
        {
            entityNotifyPropertyChanging.PropertyChanging += EntityNotifyPropertyChanging_PropertyChanging;
        }

        if (trackingObject.Entity is INotifyPropertyChanged entityNotifyPropertyChanged)
        {
            entityNotifyPropertyChanged.PropertyChanged += EntityNotifyPropertyChanged_PropertyChanged;
        }
    }
    private void UnregisterEvents(ChangeTracking trackingObject)
    {
        if (trackingObject.Entity is INotifyPropertyChanging entityNotifyPropertyChanging)
        {
            entityNotifyPropertyChanging.PropertyChanging -= EntityNotifyPropertyChanging_PropertyChanging;
        }

        if (trackingObject.Entity is INotifyPropertyChanged entityNotifyPropertyChanged)
        {
            entityNotifyPropertyChanged.PropertyChanged -= EntityNotifyPropertyChanged_PropertyChanged;
        }
    }

    private void EntityNotifyPropertyChanging_PropertyChanging(object? sender, PropertyChangingEventArgs e)
    {
        if (sender == null || e.PropertyName == null) return;
        
        var trackingObject = Get(sender);
        if (trackingObject == null)
            return;
        var propertyInfo = trackingObject.EntityType.GetProperty(e.PropertyName);
        if (propertyInfo == null)
            return;
            
        trackingObject.LastPropertyNameChanging = e.PropertyName;
        trackingObject.LastPropertyValueChanging = propertyInfo.GetMethod?.Invoke(sender, null);
    }

    private void EntityNotifyPropertyChanged_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender == null || e.PropertyName == null) return;

        var trackingObject = Get(sender);
        if (trackingObject == null)
            return;
        var propertyInfo = trackingObject.EntityType.GetProperty(e.PropertyName);
        if (propertyInfo == null)
            return;

        if (trackingObject.State != ChangeTrackingState.Updated &&
            trackingObject.State != ChangeTrackingState.Created &&
            trackingObject.State != ChangeTrackingState.Deleted &&
            trackingObject.State != ChangeTrackingState.Detached
           )
        {
            var currentValue = propertyInfo.GetMethod?.Invoke(sender, null);
            object? oldValue;

            if (typeof(INotifyPropertyChanging).IsAssignableFrom(trackingObject.EntityType))
            {
                // last property changing is too new/old
                if (trackingObject.LastPropertyNameChanging != e.PropertyName)
                {
                    return;
                }

                oldValue = trackingObject.LastPropertyValueChanging;
            }
            else // base on original entity
            {
                oldValue = propertyInfo.GetMethod?.Invoke(trackingObject.OriginalEntity, null);
            }

            if (oldValue != currentValue)
            {
                trackingObject.State = ChangeTrackingState.Updated;
            }
        }
    }

    public ChangeTracking Add(object entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityType = entity.GetType();

        var clonedEntity = DtoMapper.Map(entityType, entity);

        var trackingObject = new ChangeTracking(entity, entityType, originalEntity: clonedEntity);
        lock (_changeTracker)
        {
            _changeTracker.Add(trackingObject);
            RegisterEvents(trackingObject);
        }

        return trackingObject;
    }

    public ChangeTracking? Get(object entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return _changeTracker.FirstOrDefault(ct => ct.Entity == entity && ct.State != ChangeTrackingState.Detached);
    }

    public void Detach(ChangeTracking trackingObject)
    {
        lock (_changeTracker)
        {
            UnregisterEvents(trackingObject);
            _changeTracker.Remove(trackingObject);
        }
    }

    public void Clear()
    {
        lock (_changeTracker)
        {
            foreach (var changeTracking in _changeTracker)
            {
                UnregisterEvents(changeTracking);
            }
            _changeTracker.Clear();
        }
    }

    public IEnumerator<ChangeTracking> GetEnumerator()
    {
        return _changeTracker.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _changeTracker.GetEnumerator();
    }
}