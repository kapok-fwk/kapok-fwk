using System.ComponentModel.DataAnnotations;

namespace Kapok.Report;

public class ReportResource
{
    /// <summary>
    /// The name to identify the resource
    /// </summary>
    [Required]
    public string? Name { get; set; }

    /// <summary>
    /// The mime type of the resource
    /// </summary>
    public string? MimeType { get; set; }

    /// <summary>
    /// The binary data of the resource
    /// </summary>
    public virtual byte[]? Data { get; set; }
}