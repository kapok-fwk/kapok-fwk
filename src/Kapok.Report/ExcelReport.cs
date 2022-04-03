using System.Globalization;
using System.Text;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Kapok.Report;

/// <summary>
/// A base report for building a custom Excel report.
/// </summary>
public abstract class ExcelReport : Model.Report
{
    /// <summary>
    /// A short title which will be used as excel worksheet tap name.
    /// </summary>
    public Caption? ShortTitle { get; set; }

    /// <summary>
    /// A helper function replacing variables in a string in the C# format.
    /// <br/><b>Sample:</b>
    /// <list type="bullet">
    /// <item>inputString: <c>"Hello {Username}!"</c></item>
    /// <item>getVariableValue: <c>variableName => variableName == "Username" ? "World" : null</c></item>
    /// <item>function returns: <c>"Hello World!"</c></item>
    /// </list>
    /// </summary>
    /// <param name="inputString">
    /// The input string where the variables shall be replaced.
    /// </param>
    /// <param name="getVariableValue">
    /// A function returning a variable name value.
    /// </param>
    /// <param name="cultureInfo">
    /// (Optional) A culture info to convert the value in <paramref name="getVariableValue"/> to string
    /// in case the value implements <see cref="IFormattable"/>.
    /// </param>
    /// <returns></returns>
    internal static string? ReplaceReportParameters(string? inputString, Func<string, object?> getVariableValue, CultureInfo? cultureInfo = default)
    {
        if (inputString == null)
            return null;
        if (getVariableValue == null)
            throw new ArgumentNullException(nameof(getVariableValue));

        cultureInfo ??= CultureInfo.CurrentCulture;

        var sb = new StringBuilder();

        bool inParameter = false;
        bool inParameterFormat = false;
        var parameterName = new StringBuilder();
        var parameterFormat = new StringBuilder();

        for (int i = 0; i < inputString.Length; i++)
        {
            var c = inputString[i];

            if (c == '{')
            {
                if (inParameter)
                {
                    if (inputString[i - 1] != '{')
                    {
                        // '{' sign is used within a parameter name or parameter format; we use here optimistic behavior even the usage is strange...
                        if (inParameterFormat)
                            parameterFormat.Append(c);
                        else
                            parameterName.Append(c);
                    }
                    sb.Append(c);
                    inParameter = false;
                }
                else
                    inParameter = true;
            }
            else if (c == ':')
            {
                if (inParameter)
                {
                    inParameterFormat = true;
                }
                else
                {
                    sb.Append(c);
                }
            }
            else if (c == '}')
            {
                if (inParameter)
                {
                    if (parameterName.Length > 0)
                    {
                        var parameterNameFinal = parameterName.ToString();
                        var parameterValue = getVariableValue(parameterNameFinal);

                        if (parameterValue != null)
                        {
                            if (inParameterFormat && parameterFormat.Length > 0)
                            {
                                var parameterFormatFinal = parameterFormat.ToString();
                                if (parameterValue is IFormattable formattableValue)
                                {
                                    sb.Append(formattableValue.ToString(parameterFormatFinal, cultureInfo));
                                }
                                else
                                {
                                    sb.Append(parameterValue);
                                }
                            }
                            else
                            {
                                sb.Append(parameterValue);
                            }
                        }

                        parameterName.Clear();
                    }

                    if (parameterFormat.Length > 0)
                    {
                        parameterName.Clear();
                    }

                    inParameter = false;
                }
            }
            else
            {
                if (inParameterFormat)
                    parameterFormat.Append(c);
                else if (inParameter)
                    parameterName.Append(c);
                else
                    sb.Append(c);
            }
        }

        return sb.ToString();
    }

    // TODO: this function only uses the default parameter values; you can not edit them!
    protected string? ReplaceReportParameters(string? inputString, CultureInfo? cultureInfo = default)
    {
        return ExcelReport.ReplaceReportParameters(inputString,
            getVariableValue: name => Parameters[name].Value,
            cultureInfo: cultureInfo
        );
    }

    public virtual ExcelWorksheet Build(ReportEngine reportEngine, ExcelWorkbook workbook)
    {
        return CreateWorksheet(workbook);
    }

    protected virtual ExcelWorksheet CreateWorksheet(ExcelWorkbook workbook)
    {
        var worksheet = workbook.Worksheets.Add(ReplaceReportParameters(ShortTitle?.ToString() ?? Caption?.ToString()));

        WriteToExcelWorksheet(worksheet);

        return worksheet;
    }

    protected virtual void WriteToExcelWorksheet(ExcelWorksheet worksheet)
    {
        worksheet.Cells[1, 1].Value = ReplaceReportParameters(Caption?.ToString());
        var titleRange = worksheet.Cells[1, 1, 2, 8];
        titleRange.Merge = true;
        titleRange.Style.Font.Size = 14;
        titleRange.Style.Font.Bold = true;
        titleRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        titleRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
    }
}