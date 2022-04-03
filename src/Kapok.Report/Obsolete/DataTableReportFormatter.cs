using System.Data;
using System.Globalization;
using System.Text;
using Kapok.Report.Model;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;

namespace Kapok.Report;

[Obsolete]
public class DataTableReportFormatter : IDataTableReportFormatter
{
    private static DataTableReportFormatter _defaultFormatter;
    public static DataTableReportFormatter Default => _defaultFormatter;

    public virtual void FormatDataTable(DataTable dataTable, DataTableReport reportModel, CultureInfo cultureInfo)
    {
    }

    #region CSV formatting

    protected static char GetCsvSeparator(CultureInfo cultureInfo)
    {
        if (cultureInfo.TwoLetterISOLanguageName.ToLower() == "de")
        {
            return ';';
        }

        return ',';
    }

    private static string? ConvertValueToString(Type type, object? value, CultureInfo cultureInfo)
    {
        if (type == typeof(long) ||
            type == typeof(int) ||
            type == typeof(short) ||
            type == typeof(byte))
        {
            if (value == null || value == DBNull.Value)
                return ((long) 0).ToString(cultureInfo);

            return ((long) value).ToString(cultureInfo);
        }

        if (type == typeof(decimal) ||
            type == typeof(double) ||
            type == typeof(float))
        {
            if (value == null || value == DBNull.Value)
                return ((decimal) 0).ToString(cultureInfo);

            return ((decimal) value).ToString(cultureInfo);
        }

        if (type.IsValueType && typeof(IFormattable).IsAssignableFrom(type))
        {
            return ((IFormattable) value).ToString(null, cultureInfo);
        }

        if (value == null || value == DBNull.Value)
            return string.Empty;

        return value.ToString();
    }

    public virtual void FormatDataTableToCsv(DataTable dataTable, DataTableReport report, CultureInfo cultureInfo, Stream csvStream)
    {
        string stringSeparator = GetCsvSeparator(cultureInfo).ToString();

        using (var streamWriter = new StreamWriter(csvStream, Encoding.UTF8, 512, leaveOpen: true))
        {
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
    }

    #endregion

    #region Excel formatting

    protected static class ExcelCellDataFormat
    {
        // Source: https://stackoverflow.com/questions/20648149/what-are-numberformat-options-in-excel-vba

        public static string General => "General";
        public static string Number => "0";

        // Your custom format 
        public static string NumberDotTwoDigits => "0.00";

        public static string Currency => "$#,##0.00;[Red]$#,##0.00";
        public static string Accounting => "_($* #,##0.00_);_($* (#,##0.00);_($* \" - \"??_);_(@_)";
        public static string Date => "m/d/yy";
        public static string Time => "[$-F400] h:mm:ss am/pm";
        public static string Percentage => "0.00%";
        public static string Fraction => "# ?/?";
        public static string Scientific => "0.00E+00";
        public static string Text => "@";
        public static string Special => ";;";
        //public static string Custom => "#,##0_);[Red](#,##0)";
    }

    /// <summary>
    /// Converts a C# Type to a Excel Numberformat BuildIn integer.
    ///
    /// See also: https://github.com/JanKallman/EPPlus/blob/master/EPPlus/Style/ExcelNumberFormat.cs
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    protected static string DataTypeToExcelNumberFormat(Type type)
    {
        if (type == typeof(long) || type == typeof(ulong) ||
            type == typeof(int) || type == typeof(uint) ||
            type == typeof(short) || type == typeof(ushort) ||
            type == typeof(byte))
        {
            return ExcelCellDataFormat.Number; // Build-In format 1
        }

        if (type == typeof(decimal) ||
            type == typeof(double) ||
            type == typeof(float))
        {
            return "#,##0.00"; // Build-In format 3
        }

        return ExcelCellDataFormat.General; // Build-In format 0
    }

    // TODO: [minor feature] the excel export could be designed nicer
    //       see also: https://www.codeproject.com/Articles/1194712/Advanced-Excels-With-EPPlus

    [Obsolete("Use ExcelHelper.ToExcelWorksheet(..)")]
    public virtual void FormatDataTableToExcelWorksheet(DataTable dataTable, DataTableReport report,
        CultureInfo cultureInfo, ExcelWorksheet excelWorksheet, int rowStart = 1, int colStart = 1)
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
                cell.Value = captionClass?.Caption.LanguageOrDefault(CultureInfo.CurrentCulture) ?? col.ColumnName;

                cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                cell.Style.Border.Bottom.Style = cell.Style.Border.Top.Style = cell.Style.Border.Left.Style =
                    cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            }

            // Auto size columns
            for (int j = 0; j < dataTable.Columns.Count; j++)
                excelWorksheet.Cells[rowStart, colStart + j, rowStart + dataTable.Rows.Count + 1 /* add 1 for header line */, colStart + j].AutoFitColumns(10, 75);
        }
    }

    #endregion
}