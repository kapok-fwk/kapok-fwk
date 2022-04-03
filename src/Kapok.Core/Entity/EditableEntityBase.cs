using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kapok.Core;
using Newtonsoft.Json;

namespace Kapok.Entity;

public abstract class EditableEntityBase : EntityBase, IEditableObject, INotifyDataErrorInfo
{
    public static void SetBusinessLayerService(EditableEntityBase editableEntityBase, IBusinessLayerService service)
    {
        editableEntityBase._service = service;
    }

    protected virtual bool SetValidateProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value))
            return false;

        OnPropertyChanging(propertyName);
        ValidateProperty(value, propertyName);
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }
        
    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        _service?.OnPropertyChanged(this, propertyName);
        base.OnPropertyChanged(propertyName);
    }

    protected override void OnPropertyChanging([CallerMemberName] string? propertyName = null)
    {
        _service?.OnPropertyChanging(this, propertyName);
        base.OnPropertyChanging(propertyName);
    }

    #region IEditableObject

    private object? _backupCopy;
    private bool _inEdit;

    public virtual void BeginEdit()
    {
        if (_inEdit) return;
        _inEdit = true;
        _backupCopy = MemberwiseClone();
    }

    public virtual void CancelEdit()
    {
        if (!_inEdit) return;
        _inEdit = false;

        if (_backupCopy == null)
            return;

        // when service is not set, we assume the object to be destroyed soon, so we skip copying the backup copy back
        if (_service == null)
        {
            _backupCopy = null;
            return;
        }

        // via reflection copy all values from the backup object back to the original (=this)
        foreach (var propertyInfo in GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            // ignore all fields which are not mapped on data-level
            if (Attribute.IsDefined(propertyInfo, typeof(NotMappedAttribute)))
                continue;

            if (propertyInfo.GetMethod == null || propertyInfo.SetMethod == null)
                continue;

            CancelPropertyEdit(propertyInfo);
        }

        _backupCopy = null;
    }

    protected virtual void CancelPropertyEdit(PropertyInfo propertyInfo)
    {
        var oldValue = propertyInfo.GetMethod.Invoke(_backupCopy, new object[0]);
        propertyInfo.SetMethod.Invoke(this, new [] {oldValue});
    }

    public virtual void EndEdit()
    {
        if (!_inEdit) return;
        _inEdit = false;
        _backupCopy = null;
    }

    #endregion

    #region INotifyDataErrorInfo

    private const BindingFlags ValidateAllPropertiesBindingFlags =
        BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty;

    private IBusinessLayerService? _service;

    // gives the information if all properties have been validated at least once; if this is true, the field _validatedProperties is obsolete
    private bool _allPropertiesValidated;

    // a list of all properties which have been validated;
    // if they where not validated on GetErrors(string), a validation will be enforced then
    private HashSet<string>? _validatedProperties;

    private readonly Dictionary<string, ICollection<string>> _validationErrors = new();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void ValidateAllProperties()
    {
        if (_service == null) // we don't do any validation when the service not set; TODO maybe something to un-design (?)
            return;

        // we don't want to validate objects once more when they are already validated
        // (this requires that each property-set method executes 'ValidateProperty' when the value changed!)
        if (_allPropertiesValidated)
            return;

        var properties = GetType().GetProperties(ValidateAllPropertiesBindingFlags);

        if (_validatedProperties != null)
            properties = (from p in properties
                where !_validatedProperties.Contains(p.Name)
                select p).ToArray();

        _validatedProperties = null;
        _allPropertiesValidated = true;

        foreach (var propertyInfo in properties)
        {
            ValidateProperty(propertyInfo);
        }
    }

    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            // return errors on entity level;
            // for sake of simplicity we just return the validation errors for each property,
            // but fist we need to validate all properties

            ValidateAllProperties();

            return _validationErrors.SelectMany(p => p.Value).ToList();
        }
        else // validate a specific property
        {
            if (!_allPropertiesValidated && (_validatedProperties == null || !_validatedProperties.Contains(propertyName)))
            {
                var propertyInfo = GetType().GetProperty(propertyName, ValidateAllPropertiesBindingFlags);
                Debug.Assert(propertyInfo != null);
                ValidateProperty(propertyInfo);
            }

            if (!_validationErrors.ContainsKey(propertyName))
                return Enumerable.Empty<string>();

            return _validationErrors[propertyName];
        }
    }

    [NotMapped, JsonIgnore]
    bool INotifyDataErrorInfo.HasErrors
    {
        get
        {
            ValidateAllProperties();
            return _validationErrors.Count > 0;
        }
    }

    protected void ValidateProperty(PropertyInfo propertyInfo) =>
        ValidateProperty(propertyInfo.GetValue(this), propertyInfo.Name);

    protected virtual void ValidateProperty(object? value, [CallerMemberName] string? propertyName = null)
    {
        if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

        if (_service == null)
        {
            // When the service is not set we don't do any validation.
            // This happens e.g. in the business layer when the developer decides
            // to not set the business layer service to the entity.
            return;
        }

        if (!_allPropertiesValidated)
        {
            if (_validatedProperties == null)
                _validatedProperties = new HashSet<string>();

            _validatedProperties.Add(propertyName);
        }

        bool isValid;

        ICollection<string> validationErrors;

        try
        {
            isValid = _service.ValidateProperty(this, propertyName, value, out validationErrors);
        }
        catch (Exception e)
        {
            validationErrors = new List<string>();

            // TODO: translate this message
            validationErrors.Add("Exception during validation: "+e.Message);

            isValid = false;
        }

#if DEBUG
            if (validationErrors == null)
            {
                Debug.WriteLine("ERROR: validation error list was not set by business layer service!");
            }
            else
            {
                foreach (var businessLayerMessage in validationErrors)
                {
                    Debug.WriteLine($"Validation error: {GetType().Name}.{propertyName}: {businessLayerMessage}");
                }
            }
#endif

        if (!isValid)
        {
            if (_validationErrors.ContainsKey(propertyName))
                _validationErrors[propertyName] = validationErrors;
            else
                _validationErrors.Add(propertyName, validationErrors);
            OnErrorsChanged(propertyName);
        }
        else if (_validationErrors.ContainsKey(propertyName))
        {
            _validationErrors.Remove(propertyName);
            OnErrorsChanged(propertyName);
        }
    }

    #endregion
}