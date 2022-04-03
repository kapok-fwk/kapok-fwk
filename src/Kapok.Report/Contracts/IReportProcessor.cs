using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kapok.Report;

public interface IReportProcessor
{
    [Required]
    object? ReportModel { get; set; }

    /// <summary>
    /// Defines the language in which the report shall be exported.
    /// </summary>
    [Required]
    CultureInfo? ReportLanguage { get; set; }

    /// <summary>
    /// Contains the name of the report parameter with the value to it.
    /// </summary>
    [Required]
    Dictionary<string, object>? ParameterValues { get; set; }

    /// <summary>
    /// Validates the fields set in the report processor.
    ///
    /// This method should be called before the processing of the report begins.
    /// </summary>
    void ValidateRequiredFields();

    /// <summary>
    /// Validates the report model.
    /// </summary>
    void ValidateReportModel();
}