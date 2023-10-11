using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using Kapok.Entity;

namespace Kapok.BusinessLayer;

public class ValidatableBindableObjectBase : BindableObjectBase, INotifyDataErrorInfo
{
    protected virtual bool SetValidateProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(storage, value))
            return false;

        ValidateProperty(value, propertyName);
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #region INotifyDataErrorInfo

    private const BindingFlags ValidateAllPropertiesBindingFlags =
        BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty;

    // gives the information if all properties have been validated at least once; if this is true, the field _validatedProperties is obsolete
    private bool _allPropertiesValidated;

    // a list of all properties which have been validated;
    // if they where not validated on GetErrors(string), a validation will be enforced then
    private HashSet<string>? _validatedProperties;

    // a cache list of all property validation errors
    private readonly Dictionary<string, ICollection<BusinessLayerMessage>> _validationErrors = new();

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    private void OnErrorsChanged(string propertyName)
    {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            Debug.WriteLine(
                $"Entity {GetType().FullName}: The INotifyDataErrorInfo.GetErrors(string) method has been called with {nameof(propertyName)} null");
            return Enumerable.Empty<string>();
        }

        if (!_allPropertiesValidated && (_validatedProperties == null || !_validatedProperties.Contains(propertyName)))
        {
            var propertyInfo = GetType().GetProperty(propertyName, ValidateAllPropertiesBindingFlags);
            if (propertyInfo == null)
            {
                Debug.WriteLine($"Entity {GetType().FullName}: The INotifyDataErrorInfo.GetErrors(string) method has been called for an not existing property: {propertyName}");
                return Enumerable.Empty<string>();
            }

            ValidateProperty(propertyInfo);
        }

        if (!_validationErrors.ContainsKey(propertyName))
            return Enumerable.Empty<string>();
            
        return _validationErrors[propertyName];
    }

    [NotMapped] // JsonIgnore
    bool INotifyDataErrorInfo.HasErrors
    {
        get
        {
            if (!_allPropertiesValidated)
            {
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

            return _validationErrors.Count > 0;
        }
    }

    protected virtual void ValidatePropertyInternal(object? value, string? propertyName, ICollection<BusinessLayerMessage> validationErrors)
    {
    }

    protected void ValidateProperty(PropertyInfo propertyInfo) =>
        ValidateProperty(propertyInfo.GetValue(this), propertyInfo.Name);
        
    protected void ValidateProperty(object? value, [CallerMemberName] string? propertyName = null)
    {
        ArgumentNullException.ThrowIfNull(propertyName, nameof(propertyName));

        if (!_allPropertiesValidated)
        {
            if (_validatedProperties == null)
                _validatedProperties = new HashSet<string>();

            _validatedProperties.Add(propertyName);
        }

        ICollection<BusinessLayerMessage> validationErrors = new List<BusinessLayerMessage>();

        // do the data model validation during the server is called for the property-specific business layer validation
        var validationResults = new List<ValidationResult>();
        ValidationContext validationContext = new ValidationContext(this, null, null) { MemberName = propertyName };
        var validatorResult = Validator.TryValidateProperty(value, validationContext, validationResults);

        if (!validatorResult)
        {
            foreach (var validationResult in validationResults)
            {
                validationErrors.Add(new BusinessLayerMessage(validationResult.ErrorMessage ?? $"Validation issue of property '{propertyName}'", MessageSeverity.Error));
            }
        }

        // validation of upper class
        ValidatePropertyInternal(value, propertyName, validationErrors);

        if (validationErrors.Count != 0)
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