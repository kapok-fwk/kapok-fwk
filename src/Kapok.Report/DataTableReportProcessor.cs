using System.ComponentModel.DataAnnotations;
using System.Data;
using Kapok.Report.Model;
using OfficeOpenXml;
using System.Globalization;
using System.Text;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;


namespace Kapok.Report;

public class DataTableReportProcessor<TReportModel> : ReportProcessor<TReportModel>, IExcelWorksheetReportProcessor,
    IDataTableReportProcessor
    where TReportModel : DataTableReport
{
    #region Static members

    static DataTableReportProcessor()
    {
        ReportEngine.RegisterProcessor(typeof(DataTableReportProcessor<>), typeof(DataTableReport));
    }

    /// <summary>
    /// This method can be called to make sure that the static constructor is called.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static void Register()
    {
    }

    #endregion

    private const string MimeTypeExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    private const string MimeTypeCsv = "text/csv";

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

    private void ProcessToExcelStream(Stream stream)
    {
        ValidateRequiredFields();
        ValidateReportModel();

        var dataTable = ProcessToDataTable();

        using var p = new ExcelPackage(stream);
#pragma warning disable CS8602
        var ws = p.Workbook.Worksheets.Add(ReportModel.Caption?.LanguageOrDefault(ReportLanguage) ?? ReportModel.Name);
#pragma warning restore CS8602

        FormatDataTableToExcelWorksheet(dataTable, ReportModel, ReportLanguage, ws);

        p.Save();
    }

    public void ProcessToExcelWorksheet(ExcelWorksheet excelWorksheet, int rowStart = 1, int colStart = 1)
    {
        ValidateRequiredFields();
        ValidateReportModel();

        var dataTable = ProcessToDataTable();

#pragma warning disable CS8604
        FormatDataTableToExcelWorksheet(dataTable, ReportModel, ReportLanguage, excelWorksheet);
#pragma warning restore CS8604
    }

    private void ProcessToCsvStream(Stream stream)
    {
        var dataTable = ProcessToDataTable();

#pragma warning disable CS8604
        FormatDataTableToCsv(dataTable, ReportModel, ReportLanguage, stream);
#pragma warning restore CS8604
    }

    public override string[] SupportedMimeTypes => new[] { MimeTypeExcel, MimeTypeCsv };

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
            default:
                throw new NotSupportedException($"Unexpected mime type: {mimeType}; child processor of base class {typeof(DataTableReportProcessor<TReportModel>).FullName} has not correctly implemented the method {nameof(ProcessToStream)}.");
        }
    }


    #region CSV formatting

    private static char GetCsvSeparator(CultureInfo cultureInfo)
    {
        if (cultureInfo.TwoLetterISOLanguageName.ToLower() == "de")
        {
            return ';';
        }

        return ',';
    }

    private static string ConvertValueToString(Type type, object? value, CultureInfo cultureInfo)
    {
        if (type == typeof(long) ||
            type == typeof(int) ||
            type == typeof(short) ||
            type == typeof(byte))
        {
            if (value == null || value == DBNull.Value)
                return ((long)0).ToString(cultureInfo);

            return ((long)value).ToString(cultureInfo);
        }

        if (type == typeof(decimal) ||
            type == typeof(double) ||
            type == typeof(float))
        {
            if (value == null || value == DBNull.Value)
                return ((decimal)0).ToString(cultureInfo);

            return ((decimal)value).ToString(cultureInfo);
        }
        
        if (type.IsValueType && typeof(IFormattable).IsAssignableFrom(type))
        {
            if (value == null)
                return string.Empty;

            return ((IFormattable)value).ToString(null, cultureInfo);
        }

        if (value == null || value == DBNull.Value)
            return string.Empty;

        return value.ToString() ?? string.Empty;
    }

    private void FormatDataTableToCsv(DataTable dataTable, DataTableReport report, CultureInfo cultureInfo, Stream csvStream)
    {
        string stringSeparator = GetCsvSeparator(cultureInfo).ToString();

        using var streamWriter = new StreamWriter(csvStream, Encoding.UTF8, 512, leaveOpen: true);
        streamWriter.NewLine = "\n"; // write just LF, not CR LF as .NET does by default

        if (dataTable.Columns.Count > 0)
        {
            var columnNames = from dataColumn in dataTable.Columns.Cast<DataColumn>()
                select report.Fields?.SingleOrDefault(c => c.Name == dataColumn.ColumnName)
                           ?.Caption.LanguageOrDefault(CultureInfo.CurrentCulture)
                       ?? dataColumn.ColumnName;

            streamWriter.WriteLine(string.Join(stringSeparator, columnNames));

            List<string> fields = new List<string>(dataTable.Columns.Count);

            foreach (DataRow row in dataTable.Rows)
            {
                foreach (DataColumn column in dataTable.Columns)
                {
                    fields.Add(
                        ConvertValueToString(column.DataType, row[column], cultureInfo)
                    );
                }

                streamWriter.WriteLine(string.Join(stringSeparator, fields));
                fields.Clear();
            }
        }
    }

    #endregion

    #region Excel formatting

    /// <summary>
    /// Converts a C# Type to a Excel Number format BuildIn integer.
    ///
    /// See also: https://github.com/JanKallman/EPPlus/blob/master/EPPlus/Style/ExcelNumberFormat.cs
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static string DataTypeToExcelNumberFormat(Type type)
    {
        if (type == typeof(long) || type == typeof(ulong) ||
            type == typeof(int) || type == typeof(uint) ||
            type == typeof(short) || type == typeof(ushort) ||
            type == typeof(byte))
        {
            return ExcelHelper.ExcelCellDataFormat.Number; // Build-In format 1
        }

        if (type == typeof(decimal) ||
            type == typeof(double) ||
            type == typeof(float))
        {
            return "#,##0.00"; // Build-In format 3
        }

        return ExcelHelper.ExcelCellDataFormat.General; // Build-In format 0
    }

    // TODO: [minor feature] the excel export could be designed nicer
    //       see also: https://www.codeproject.com/Articles/1194712/Advanced-Excels-With-EPPlus

    private void FormatDataTableToExcelWorksheet(DataTable dataTable, DataTableReport report,
        CultureInfo cultureInfo, ExcelWorksheet excelWorksheet, [Range(1, int.MaxValue)] int rowStart = 1,
        [Range(1, int.MaxValue)] int colStart = 1)
    {
        if (rowStart < 1) throw new ArgumentException($"The parameter {nameof(rowStart)} is {rowStart}. The value must be greater than zero.", nameof(rowStart));
        if (colStart < 1) throw new ArgumentException($"The parameter {nameof(colStart)} is {colStart}. The value must be greater than zero.", nameof(colStart));

        excelWorksheet.Cells.Style.Font.Size = 11;
        excelWorksheet.Cells.Style.Font.Name = "Calibri";

        //Merging cells and create a center heading for out table

        if (dataTable.Columns.Count > 0)
        {
            // fill rows from dataTable
            excelWorksheet.Cells[rowStart, colStart].LoadFromDataTable(dataTable, true, TableStyles.Light1);

            // override the header with new captions and style
            int n = 0;
            foreach (DataColumn col in dataTable.Columns)
            {
                excelWorksheet.Column(colStart + n).Style.Numberformat.Format = DataTypeToExcelNumberFormat(col.DataType);

                var cell = excelWorksheet.Cells[rowStart, colStart + n++];

                var captionClass = report.Fields?.SingleOrDefault(c => c.Name == col.ColumnName);
                cell.Value = captionClass?.Caption?.LanguageOrDefault(cultureInfo) ?? col.ColumnName;

                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                cell.Style.Border.Bottom.Style = cell.Style.Border.Top.Style = cell.Style.Border.Left.Style =
                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            // Auto size columns
            for (int j = 0; j < dataTable.Columns.Count; j++)
            {
                excelWorksheet.Cells[
                    rowStart,
                    colStart + j,
                    rowStart + dataTable.Rows.Count + 1 /* add 1 for header line */,
                    colStart + j
                ].AutoFitColumns(10, 75);
            }
        }
    }

    #endregion
}