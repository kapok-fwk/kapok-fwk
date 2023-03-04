namespace Kapok.BusinessLayer;

public interface IBusinessLayerService
{
    void OnPropertyChanging(object entry, string? propertyName);

    void OnPropertyChanged(object entry, string? propertyName);

    bool ValidateProperty(object entry, string propertyName, object? value, out ICollection<string>? validationErrors);
}