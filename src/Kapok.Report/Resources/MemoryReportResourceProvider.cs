using System.Collections;

namespace Kapok.Report;

/// <summary>
/// Resources for reports stored in memory
/// </summary>
public class MemoryReportResourceProvider : IReportResourceProvider
{
    private readonly Dictionary<string, ReportResource> _reportResources = new();

    public IEnumerator<ReportResource> GetEnumerator()
    {
        return _reportResources.Values.GetEnumerator();
    }

    public void Add(ReportResource resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        if (string.IsNullOrEmpty(resource.Name))
            throw new ArgumentException("The resource must have a name (property Name)");

        if (_reportResources.ContainsKey(resource.Name))
            throw new ArgumentException($"A resource with name '{resource.Name}' exist already");

        _reportResources.Add(resource.Name, resource);
    }

    public void Clear()
    {
        _reportResources.Clear();
    }

    public bool Contains(ReportResource? item)
    {
        if (item == null || item.Name == null)
            return false;

        return _reportResources.ContainsKey(item.Name);
    }

    public bool Remove(ReportResource? item)
    {
        if (item == null || item.Name == null)
            return false;

        return _reportResources.Remove(item.Name);
    }

    public ReportResource this[string resourceName]
    {
        get => _reportResources[resourceName];
        set => _reportResources[resourceName] = value;
    }

    public int Count => _reportResources.Count;
    public bool IsReadOnly => false;

    public ReportResource? TryGet(string? resourceName)
    {
        if (resourceName == null)
            return null;

        if (_reportResources.ContainsKey(resourceName))
            return null;

        return _reportResources[resourceName];
    }

    #region IEnumerator

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region ICollection<ReportResource>

    void ICollection<ReportResource>.CopyTo(ReportResource[] array, int arrayIndex)
    {
        _reportResources.Values.CopyTo(array, arrayIndex);
    }

    #endregion
}