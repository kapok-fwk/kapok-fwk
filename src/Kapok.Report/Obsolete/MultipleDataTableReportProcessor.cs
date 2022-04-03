using System.Data;
using System.Globalization;
using System.Reflection;
using Kapok.Report.Model;
using OfficeOpenXml;

namespace Kapok.Report;

// TODO: process to xml is not implemented yet (see IXmlReportProcessor)
[Obsolete]
public class MultipleDataTableReportProcessor : ReportProcessor<MultipleDataTableReport>, IExcelReportProcessor
{
    private const string MimeTypeExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private Dictionary<object, dynamic> _subReportProcessors;
    protected Dictionary<object, dynamic> SubReportProcessors
    {
        get
        {
            if (_subReportProcessors == null && ReportModel != null)
            {
                _subReportProcessors = new Dictionary<object, dynamic>();

                foreach (dynamic subReport in ReportModel.SubReports)
                {
                    _subReportProcessors.Add(subReport, GetProcessorFromReport(subReport));
                }
            }

            return _subReportProcessors;
        }
    }

    public IMultipleDataTableReportFormatter ReportFormatter { get; set; }

    private IMultipleDataTableReportFormatter LoadFormatter()
    {
        return ReportFormatter ?? MultipleDataTableReportFormatter.Default;
    }

    private DataTableReportProcessor<TReport> GetProcessorFromReport<TReport>(TReport report)
        where TReport : DataTableReport
    {
        if (report == null) throw new ArgumentNullException(nameof(report));

        var attribute = report.GetType().GetCustomAttribute<ReportProcessorAttribute>();
        if (attribute == null)
            throw new ArgumentException($"The given report type {typeof(TReport).FullName} does not implement the attribute {typeof(ReportProcessorAttribute).FullName}.\nIt is not possible to use this type with a {nameof(MultipleDataTableReport)}.", nameof(report));

        var processor = (DataTableReportProcessor<TReport>)Activator.CreateInstance(attribute.ProcessorType);

        processor.ReportLanguage = ReportLanguage;
        processor.ReportModel = report;

        // pass over the parameter values
        Dictionary<string, object> paramValues = new Dictionary<string, object>();

        if (processor.ReportModel.Parameters != null)
        {
            foreach (var parameter in processor.ReportModel.Parameters)
            {
                if (ParameterValues.ContainsKey(parameter.Name))
                {
                    paramValues.Add(parameter.Name, ParameterValues[parameter.Name]);
                }
                else
                {
                    paramValues.Add(parameter.Name, parameter.DefaultValue);
                }
            }
        }
            
        processor.ParameterValues = paramValues;
            
        ReportModel?.InitializeProcessorDelegate(report, processor);

        return processor;
    }
        
    public override void ValidateRequiredFields()
    {
        base.ValidateRequiredFields();

        foreach (dynamic reportProcessor in SubReportProcessors.Values)
        {
            reportProcessor.ValidateReportModel();
        }
    }

    public override void ValidateReportModel()
    {
        base.ValidateReportModel();

        foreach (dynamic reportProcessor in SubReportProcessors.Values)
        {
            reportProcessor.ValidateReportModel();
        }
    }

    public override string[] SupportedMimeTypes => new[] {MimeTypeExcel};

    public override void ProcessToStream(string mimeType, Stream stream)
    {
        TestMimeType(mimeType);

        if (mimeType == MimeTypeExcel)
            ProcessToExcelStream(stream);
        else
            throw new NotSupportedException($"Unexpected mime type: {mimeType}; child processor of base class {typeof(MultipleDataTableReportProcessor).FullName} has not correctly implemented the method {nameof(ProcessToStream)}.");
    }

    public void ProcessToExcelStream(Stream stream)
    {
        ValidateRequiredFields();
        ValidateReportModel();

        var resultDataTables = new List<Tuple<DataTableReport,DataTable, IDataTableReportFormatter>>();

        foreach (dynamic subReport in ReportModel.SubReports)
        {
            dynamic reportProcessor = GetProcessorFromReport(subReport);

            var dataTable = reportProcessor.ProcessToDataTable();

            resultDataTables.Add(new Tuple<DataTableReport, DataTable, IDataTableReportFormatter>(
                subReport, dataTable, (IDataTableReportFormatter)reportProcessor.ReportFormatter));
        }

        ProcessToExcelStream(resultDataTables, stream, ReportLanguage);
    }

    /// <summary>
    /// Create a excel sheet stream out of several report results.
    /// </summary>
    /// <param name="reportResults"></param>
    /// <param name="stream"></param>
    /// <param name="cultureInfo"></param>
    protected virtual void ProcessToExcelStream(IReadOnlyList<Tuple<DataTableReport, DataTable, IDataTableReportFormatter>> reportResults, Stream stream, CultureInfo cultureInfo)
    {
        var formatter = LoadFormatter();

        using (var p = new ExcelPackage(stream))
        {
            formatter.FormatDataTableToExcelWorkbook(
                reportResults, cultureInfo, p.Workbook);

            p.Save();
        }
    }
}