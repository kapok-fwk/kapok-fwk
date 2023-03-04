namespace Kapok.Report;

/// <summary>
/// A resource based on a file
/// </summary>
public class FileReportResource : ReportResource
{
    public string FullFilePath { get; }

    public FileReportResource(string name, string fullPath)
    {
        Name = name;
        FullFilePath = fullPath;
    }

    public override byte[]? Data
    {
        get => File.ReadAllBytes(FullFilePath);
        set
        {
            if (value != null)
                File.WriteAllBytes(FullFilePath, value);
        }
    }
}