using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Reflection;
using OfficeOpenXml;
using OfficeOpenXml.Table;

namespace Kapok.Report;

public static class ExcelHelper 
{
    private static class ExcelCellDataFormat
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
    /// Returns a DataTable from an enumeration and column PropertyInfo list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    /// <param name="columnProperties"></param>
    /// <returns></returns>
    private static DataTable ToDataTable<T>(IEnumerable<T> data, PropertyInfo[] columnProperties)
    {
        var dataTable = new DataTable();

        foreach (var columnProperty in columnProperties)
        {
            dataTable.Columns.Add(columnProperty.Name,
                Nullable.GetUnderlyingType(columnProperty.PropertyType) ?? columnProperty.PropertyType);
        }

        foreach (var row in data)
        {
            var newRow = dataTable.NewRow();

            foreach (var columnProperty in columnProperties)
            {
                object? value = null;

                if (columnProperty.CanRead)
                    value = columnProperty.GetMethod.Invoke(row, Array.Empty<object>());

                value ??= DBNull.Value;

                newRow[columnProperty.Name] = value;
            }

            dataTable.Rows.Add(newRow);
        }

        return dataTable;
    }

    /// <summary>
    /// Loads the rows from a report data set into a excel table starting at <para>range</para>.
    /// </summary>
    /// <param name="reportDataSet">
    /// The report data set which data shall be written to excel.
    /// </param>
    /// <param name="worksheet">
    /// The worksheet the IReportDataSet is written to.
    /// </param>
    /// <param name="startRow">
    /// The start row position of the content.
    /// </param>
    /// <param name="startColumn">
    /// The start column position of the content.
    /// </param>
    /// <param name="columnProperties">
    /// The properties which content to write.
    /// </param>
    /// <param name="writeHeader">
    /// If the header shall be written into Excel. If yes, the procedure will use
    /// the display property.
    /// </param>
    /// <param name="name">
    /// Name of the data table.
    /// </param>
    /// <param name="tableStyle">
    /// The table style to be used. If not given, the data will be written but not be transformed into
    /// a excel table.
    /// </param>
    /// <param name="cultureInfo">
    /// The culture to be used when the DisplayAttribute uses a resource.
    /// </param>
    /// <param name="excelWorkbook"></param>
    /// <returns>
    /// Returns the number of rows added.
    /// </returns>
    public static int ToExcelWorksheet<T>(this IReportDataSet reportDataSet,
        ExcelWorksheet worksheet, int startRow, int startColumn,
        PropertyInfo[] columnProperties,
        bool writeHeader, string? name = null, TableStyles? tableStyle = null, CultureInfo? cultureInfo = null, ExcelWorkbook? excelWorkbook = null)
    {
        var dataTable = ToDataTable(reportDataSet.Cast<T>(), columnProperties);

        if (dataTable.Columns.Count == 0)
            return 0; // we don't continue here because range.LoadFromDataTable(..) requires at least one column

        var range = worksheet.Cells[startRow, startColumn];

        // fill rows from dataTable
        if (tableStyle.HasValue && name == null)
            range.LoadFromDataTable(dataTable, writeHeader, tableStyle.Value);
        else
            range.LoadFromDataTable(dataTable, writeHeader);

        if (writeHeader)
        {
            // overrides the property name with the display attribute name if given, otherwise
            // use the default name (which is the property name)
            int n = 0;
            foreach (var columnProperty in columnProperties)
            {
                var columnCell = worksheet.Cells[startRow, startColumn + n++];
                columnCell.Value = columnProperty.GetDisplayAttributeNameOrDefault(cultureInfo);
            }
        }

        if (writeHeader && name != null)
        {
            var table = worksheet.Tables.Add(worksheet.Cells[
                startRow,
                startColumn,
                startRow + dataTable.Rows.Count + 1 /* header row */,
                startColumn + dataTable.Columns.Count
            ], name);
            table.TableStyle = TableStyles.Light1;

            if (excelWorkbook != null)
            {
                var dateStyle = excelWorkbook.Styles.CreateNamedStyle("Kapok_Date");
                dateStyle.Style.Numberformat.Format = "mm-dd-yy"; // Built in Format 13 "Date"

                //var textStyle = excelWorkbook.Styles.CreateNamedStyle("Kapok_Text");
                //textStyle.Style.Numberformat.Format = ExcelCellDataFormat.Text;

                var numberStyle = excelWorkbook.Styles.CreateNamedStyle("Kapok_Number");
                numberStyle.Style.Numberformat.Format = ExcelCellDataFormat.Accounting;

                /* key = format string, value = number style name */
                Dictionary<string, string> customFormats = new Dictionary<string, string>();
                int nextCustomStyleNum = 1;


                int n = 0;
                foreach (var columnProperty in columnProperties)
                {
                    string? styleName = null;

                    if (columnProperty.PropertyType == typeof(bool))
                    {
                        // do nothing, this is already well taken care of within the LoadFromDataTable(..) function
                    }
                    else if (columnProperty.PropertyType == typeof(string))
                    {
                        // do nothing, this is already well taken care of within the LoadFromDataTable(..) function
                    }
                    else if (columnProperty.PropertyType == typeof(DateTime))
                    {
                        var dataType = columnProperty.GetCustomAttribute<DataTypeAttribute>()?.DataType;
                        if (dataType != null)
                        {
                            switch (dataType)
                            {
                                case DataType.Date:
                                    styleName = dateStyle.Name;
                                    break;
                            }
                        }
                    }
                    else if (columnProperty.PropertyType == typeof(int) || columnProperty.PropertyType == typeof(long) ||
                             columnProperty.PropertyType == typeof(float) || columnProperty.PropertyType == typeof(double) || columnProperty.PropertyType == typeof(decimal))
                    {
                        var format = columnProperty.GetCustomAttribute<DisplayFormatAttribute>()?.DataFormatString;

                        if (format != null)
                        {
                            if (!customFormats.ContainsKey(format))
                            {
                                var customStyle = excelWorkbook.Styles.CreateNamedStyle($"Kapok_Custom_{nextCustomStyleNum++}");
                                customStyle.Style.Numberformat.Format = format; // todo redesign format to excel format

                                customFormats.Add(format, customStyle.Name);
                            }

                            styleName = customFormats[format];
                        }
                        else
                        {
                            styleName = numberStyle.Name;
                        }
                    }

                    if (styleName != null)
                        table.Columns[n].DataCellStyleName = styleName;

                    n++;
                }
            }
        }

        return dataTable.Rows.Count;
    }
}