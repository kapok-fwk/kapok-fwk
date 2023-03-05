using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Kapok.Report;

public abstract class ReportProcessor<TReportModel> : IMimeTypeReportProcessor
    where TReportModel : Model.Report
{
    protected ReportProcessor()
    {
        ReportLanguage = CultureInfo.CurrentUICulture;
    }

    [Required]
    public TReportModel? ReportModel { get; set; }

    /// <summary>
    /// Defines the language in which the report shall be exported.
    /// </summary>
    public CultureInfo ReportLanguage { get; set; }

    /// <summary>
    /// Contains the name of the report parameter with the value to it.
    /// </summary>
    public Dictionary<string, object> ParameterValues { get; set; } = new();

    /// <summary>
    /// Validates the fields set in the report processor.
    ///
    /// This method should be called before the processing of the report begins.
    /// </summary>
    public virtual void ValidateRequiredFields()
    {
        if (ReportModel == null)
            throw new NotSupportedException($"The report model was not given to the class. Property: {nameof(ReportModel)}");

        // TODO: need validation if fields of the model with [Required] are not filled.
    }

    /// <summary>
    /// Validates the report model.
    /// </summary>
    public virtual void ValidateReportModel()
    {
        if (ReportModel == null)
            throw new NotSupportedException("The report model is not set");
        if (ReportModel?.Name == null)
            throw new NotSupportedException($"The report model has no name. Property: {nameof(Model.Report.Name)}");
    }

    public abstract string[] SupportedMimeTypes { get; }

    protected void TestMimeType(string mimeType)
    {
        if (!SupportedMimeTypes.Contains(mimeType))
            throw new ArgumentException($"The processor class {GetType().FullName} does not support the mime type {mimeType}. Supported mime types are: {string.Join(", ", SupportedMimeTypes)}", nameof(mimeType));
    }

    /// <summary>
    /// Process the data and generates an stream-output
    /// </summary>
    /// <param name="mimeType">
    /// The destination mime type.
    /// </param>
    /// <param name="stream">
    /// The destination stream.
    /// </param>
    public abstract void ProcessToStream(string mimeType, Stream stream);

    #region IReportProcessor

    object? IReportProcessor.ReportModel
    {
        get => ReportModel;
        set => ReportModel = (TReportModel?) value;
    }

    #endregion
}