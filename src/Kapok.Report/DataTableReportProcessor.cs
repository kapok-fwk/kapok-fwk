using System.Data;
using System.Xml.Serialization;
using Kapok.Report.Model;
using Kapok.Report.Xml;
using OfficeOpenXml;

namespace Kapok.Report;

public class DataTableReportProcessor<TReportModel> : ReportProcessor<TReportModel>, IDataTableReportProcessor,
    IExcelReportProcessor, IExcelWorksheetReportProcessor, ICsvReportProcessor
    where TReportModel : DataTableReport
{
    #region Static members

    static DataTableReportProcessor()
    {
        ReportEngine.RegisterProcessor(typeof(DataTableReportProcessor<>), typeof(DataTableReport));
    }

    public static void Register()
    {
        // this function can be called to make sure that the static constructor is called.
    }

    #endregion

    private const string MimeTypeExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string MimeTypeCsv = "text/csv";
    private const string MimeTypeXml = "text/xml";

    [Obsolete]
    public IDataTableReportFormatter ReportFormatter { get; set; }

    public virtual DataTable ProcessToDataTable()
    {
        ValidateRequiredFields();
        ValidateReportModel();
            
        IDataTableReportDataSet? currentDataSet = null;

#pragma warning disable CS8602
        foreach (var dataSet in ReportModel.DataSets.Values)
#pragma warning restore CS8602
        {
            if (dataSet is IDataTableReportDataSet dataTableReportDataSet)
            {
                currentDataSet = dataTableReportDataSet;
                break;
            }
        }

        if (currentDataSet == null)
            throw new NotSupportedException(
                $"The report must have at least one report data set implementing type {typeof(IDataTableReportDataSet).FullName}");

        throw new NotImplementedException();
    }

    [Obsolete]
    private IDataTableReportFormatter LoadFormatter()
    {
        return ReportFormatter ?? DataTableReportFormatter.Default;
    }

    public virtual void ProcessToExcelStream(Stream stream)
    {
        ValidateRequiredFields();
        ValidateReportModel();

        var formatter = LoadFormatter();
        var dataTable = ProcessToDataTable();

        using (var p = new ExcelPackage(stream))
        {
#pragma warning disable CS8602
            var ws = p.Workbook.Worksheets.Add(ReportModel.Caption?.LanguageOrDefault(ReportLanguage) ?? ReportModel.Name);
#pragma warning restore CS8602

            formatter.FormatDataTableToExcelWorksheet(dataTable, ReportModel, ReportLanguage, ws);

            p.Save();
        }
    }
        
    public virtual void ProcessToExcelWorksheet(ExcelWorksheet excelWorksheet, int rowStart = 1, int colStart = 1)
    {
        ValidateRequiredFields();
        ValidateReportModel();

        var formatter = LoadFormatter();
        var dataTable = ProcessToDataTable();

#pragma warning disable CS8604
        formatter.FormatDataTableToExcelWorksheet(dataTable, ReportModel, ReportLanguage, excelWorksheet);
#pragma warning restore CS8604
    }

    public override void ValidateReportModel()
    {
        base.ValidateReportModel();

#pragma warning disable CS8602
        if (ReportModel.Fields == null)
#pragma warning restore CS8602
            throw new NotSupportedException($"The report {ReportModel.Name} does not have any out fields. Property: {nameof(DataTableReport.Fields)}");
    }

    [Obsolete]
    protected ReportXmlTableResult DataTableToXmlModel(DataTable dataTable)
    {
        ReportXmlTableResult rr = new ReportXmlTableResult();

        rr.Head = new ReportXmlTableResult.HeadDefinition();
        rr.Head.Name = dataTable.TableName;
        rr.Head.MetaLines = new List<ReportXmlTableResult.HeadDefinition.MetaDefinition>();
        rr.Head.MetaLines.Add(new ReportXmlTableResult.HeadDefinition.MetaDefinition
        {
            Name = "ProcessDateTime",
            Value = DateTime.Now.ToString("u")
        });
        rr.Body = new ReportXmlTableResult.BodyDefinition();
        rr.Body.Tables = new List<ReportXmlTableResult.BodyDefinition.TableDefinition>();

        var tableResult = new ReportXmlTableResult.BodyDefinition.TableDefinition();
        tableResult.Columns = new List<ReportXmlTableResult.BodyDefinition.TableDefinition.ColumnDefinition>();

        foreach (var reportModelField in ReportModel.Fields)
        {
            tableResult.Columns.Add(new ReportXmlTableResult.BodyDefinition.TableDefinition.ColumnDefinition
            {
                Name = reportModelField.Name,
                Caption = reportModelField.Caption.LanguageOrDefault(ReportLanguage)
            });
        }

        tableResult.Rows = new List<ReportXmlTableResult.BodyDefinition.TableDefinition.RowDefinition>();

        foreach (DataRow dataTableRow in dataTable.Rows)
        {
            var newRow = new ReportXmlTableResult.BodyDefinition.TableDefinition.RowDefinition();
            newRow.Value = new List<object>();

            foreach (var reportModelField in ReportModel.Fields)
            {
                newRow.Value.Add(dataTableRow[reportModelField.Name]);
            }

            tableResult.Rows.Add(newRow);
        }

        rr.Body.Tables.Add(tableResult);

        return rr;
    }

    public virtual void ProcessToCsvStream(Stream stream)
    {
        var formatter = LoadFormatter();
        var dataTable = ProcessToDataTable();
            
        formatter.FormatDataTableToCsv(dataTable, ReportModel, ReportLanguage, stream);
    }

    [Obsolete]
    public virtual void ProcessToXmlStream(Stream stream)
    {
        var dataTable = ProcessToDataTable();
        var xmlModel = DataTableToXmlModel(dataTable);

        var serializer = new XmlSerializer(typeof(ReportXmlTableResult));
        serializer.Serialize(stream, xmlModel);
    }

    public override string[] SupportedMimeTypes => new[] {MimeTypeExcel, MimeTypeCsv};

    public override void ProcessToStream(string mimeType, Stream stream)
    {
        TestMimeType(mimeType);

        switch (mimeType)
        {
            case MimeTypeExcel:
                ProcessToExcelStream(stream);
                break;
            case MimeTypeCsv:
                ProcessToCsvStream(stream);
                break;
            case MimeTypeXml:
                ProcessToXmlStream(stream);
                break;
            default:
                throw new NotSupportedException($"Unexpected mime type: {mimeType}; child processor of base class {typeof(DataTableReportProcessor<TReportModel>).FullName} has not correctly implemented the method {nameof(ProcessToStream)}.");
        }
    }
}