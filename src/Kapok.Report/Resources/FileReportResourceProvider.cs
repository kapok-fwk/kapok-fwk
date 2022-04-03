using System.Collections;

namespace Kapok.Report;

/// <summary>
/// A report resource provider based on a file directory
/// </summary>
public class DirectoryReportResourceProvider : IReportResourceProvider
{
    public DirectoryReportResourceProvider(string assetBasePath, string searchPattern = "*")
    {
        AssetBasePath = Path.GetFullPath(assetBasePath);
        SearchPattern = searchPattern;
    }

    public string AssetBasePath { get; }

    public string SearchPattern { get; }

    protected string GetResourceFullPath(string resourceName) => Path.Combine(AssetBasePath, resourceName);

    protected string GetResourceNameFromFullPath(string fullPath)
    {
        if (fullPath.StartsWith(AssetBasePath))
            return fullPath.Substring(AssetBasePath.Length);

        return fullPath;
    }

    private IEnumerable<string> GetAllFiles()
    {
        return Directory.EnumerateFiles(AssetBasePath, SearchPattern, SearchOption.AllDirectories);
    }

    public IEnumerator<ReportResource> GetEnumerator()
    {
        return (from fullPath in GetAllFiles()
            select (ReportResource)new FileReportResource(GetResourceNameFromFullPath(fullPath), fullPath)).AsEnumerable().GetEnumerator();
    }

    public void Add(ReportResource? item)
    {
        if (item == null)
            return;

        if (Contains(item))
            throw new ArgumentException($"A resource with the name {item.Name} exist already.", nameof(item));

        File.WriteAllBytes(GetResourceFullPath(item.Name), item.Data);
    }

    public void Clear()
    {
        throw new NotSupportedException($"The method {nameof(Clear)} is not supported by class {typeof(DirectoryReportResourceProvider).FullName}");
    }

    public bool Contains(ReportResource? item)
    {
        if (item == null)
            return false;
        if (string.IsNullOrEmpty(item.Name))
            return false;

        return File.Exists(GetResourceFullPath(item.Name));
    }

    public bool Remove(ReportResource? item)
    {
        if (!Contains(item))
            return false;

#pragma warning disable 8602
        File.Delete(GetResourceFullPath(item.Name));
#pragma warning restore 8602
        return true;
    }

    public int Count => GetAllFiles().Count();

    public bool IsReadOnly => false;

    public ReportResource this[string resourceName]
    {
        get
        {
            if (resourceName == null)
                throw new ArgumentNullException(nameof(resourceName));

            string fullPath = GetResourceFullPath(resourceName);
            if (!File.Exists(fullPath))
                throw new ArgumentException($"Could not find resource/file with name {resourceName}.\nFull path: {fullPath}");

            return new FileReportResource(resourceName, fullPath);
        }
        set
        {
            if (resourceName == null)
                throw new ArgumentNullException(nameof(resourceName));

            Add((FileReportResource) value);
        }
    }

    public ReportResource? TryGet(string? resourceName)
    {
        if (resourceName == null)
            return null;

        string fullPath = GetResourceFullPath(resourceName);
        if (!File.Exists(fullPath))
            return null;

        return new FileReportResource(resourceName, fullPath);
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
        this.ToArray().CopyTo(array, arrayIndex);
    }

    #endregion
}