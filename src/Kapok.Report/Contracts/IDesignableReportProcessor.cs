using Kapok.View;

namespace Kapok.Report;

/// <summary>
/// Identifies an processor which can be configured
/// to report different output objects.
/// </summary>
public interface IDesignableReportProcessor : IReportProcessor
{
    /// <summary>
    /// The binary data of the design.
    /// </summary>
    byte[]? CurrentDesign { get; set; }

    /// <summary>
    /// Opens an UI dialog to design the report.
    ///
    /// Returns <code>true</code> when the design has changed,
    /// otherwise <code>false</code>
    /// </summary>
    bool OpenDesignDialog(IViewDomain viewDomain);
}