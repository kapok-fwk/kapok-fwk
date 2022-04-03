using System.Data;
using System.Linq.Dynamic.Core;
using System.Text;

namespace Kapok.Report;

[Obsolete]
public class LinqQueryableReportProcessor : DataTableReportProcessor<Model.LinqQueryableReport>
{
    public override void ValidateReportModel()
    {
        base.ValidateReportModel();

        if (ReportModel.QueryableObject == null)
            throw new NotSupportedException($"The property {nameof(ReportModel.QueryableObject)} was not given in the report model.");
    }

    public override void ProcessToStream(string mimeType, Stream stream)
    {
        TestMimeType(mimeType);

        base.ProcessToStream(mimeType, stream);
    }

    public IQueryable ProcessToQueryable()
    {
        ValidateRequiredFields();
        ValidateReportModel();

        var query = ReportModel.QueryableObject;

        if (!string.IsNullOrWhiteSpace(ReportModel.WhereClause))
        {
            StringBuilder newWhereClause = new StringBuilder(ReportModel.WhereClause.Length);

            int pos = 0;
            string variable = null;
            int variableCounter = 0;
            List<object> paramValues = new List<object>();
            while (pos < ReportModel.WhereClause.Length)
            {
                Char currentChar = ReportModel.WhereClause[pos];

                if (currentChar == '@')
                {
                    newWhereClause.Append(currentChar);
                    variable = string.Empty;
                }
                else if (variable != null)
                {
                    bool variableEnd = false;
                    bool addChar = false;

                    if (currentChar >= 'a' && currentChar <= 'z' ||
                        currentChar >= 'A' && currentChar <= 'Z' ||
                        currentChar >= '0' && currentChar <= '0')
                    {
                        variable += currentChar;
                    }
                    else
                    {
                        addChar = true;
                        variableEnd = true;
                    }

                    if (!variableEnd && pos == ReportModel.WhereClause.Length - 1)
                    {
                        variableEnd = true;
                    }

                    if (variableEnd)
                    {
                        newWhereClause.Append((variableCounter++).ToString());

                        if (ParameterValues == null || !ParameterValues.ContainsKey(variable))
                        {
                            throw new NotSupportedException($"The variable @{variable} has been used in the where clause of the report but was not passed over to the report.");
                        }

                        paramValues.Add(ParameterValues[variable]);

                        variable = null;
                    }

                    if (addChar)
                        newWhereClause.Append(currentChar);
                }
                else
                {
                    newWhereClause.Append(currentChar);
                }

                pos++;
            }

            try
            {
                query = query.Where(newWhereClause.ToString(), paramValues.ToArray());
            }
            catch (System.Linq.Dynamic.Core.Exceptions.ParseException e)
            {
                throw new NotSupportedException("Failure in where clause.", e);
            }
        }
            
        if (!string.IsNullOrWhiteSpace(ReportModel.GroupByClause))
        {
            try
            {
                query = query.GroupBy(ReportModel.GroupByClause, "it");
            }
            catch (System.Linq.Dynamic.Core.Exceptions.ParseException e)
            {
                throw new NotSupportedException("Failure in group by clause.", e);
            }
        }

        if (!string.IsNullOrWhiteSpace(ReportModel.OrderByClause))
        {
            try
            {
                query = query.OrderBy(ReportModel.OrderByClause);
            }
            catch (System.Linq.Dynamic.Core.Exceptions.ParseException e)
            {
                throw new NotSupportedException("Failure in order by clause.", e);
            }
        }

        if (ReportModel.Take != default(int))
        {
            query = query.Take(ReportModel.Take);
        }

        var selectClause = ReportModel.SelectClause;

        if (string.IsNullOrWhiteSpace(selectClause))
        {
            StringBuilder innerSelectClause = new StringBuilder();
            foreach (var reportField in ReportModel.Fields)
            {
                if (innerSelectClause.Length != 0)
                {
                    innerSelectClause.Append(",");
                }

                innerSelectClause.Append(reportField.SourceName == reportField.Name
                    ? $"{reportField.SourceName}"
                    : $"{reportField.SourceName} as {reportField.Name}");
            }

            if (innerSelectClause.Length == 0)
                throw new NotSupportedException(
                    $"The report {ReportModel.Name} does not have a field with a {nameof(Model.DataSetField.SourceName)}.");

            selectClause = $"new ({selectClause})";
        }

        try
        {
            query = query.Select(selectClause);
        }
        catch (System.Linq.Dynamic.Core.Exceptions.ParseException e)
        {
            throw new NotSupportedException("Failure in select clause.", e);
        }

        return query;
    }

    public override DataTable ProcessToDataTable()
    {
        var query = ProcessToQueryable();

        DataTable dataTable = new DataTable(ReportModel.Name);

        bool columnsCreated = false;

        Type type = null;
        foreach (var entry in query)
        {
            if (!columnsCreated)
            {
                type = entry.GetType();

                foreach (var reportField in ReportModel.Fields)
                {
                    var propertyInfo = type.GetProperty(reportField.SourceName);
                    if (propertyInfo == null)
                        throw new NotSupportedException(
                            $"Could not find field {reportField.Name} (SourceName: {reportField.SourceName}) in the data entry.");

                    dataTable.Columns.Add(reportField.Name,
                        propertyInfo.PropertyType);
                }

                columnsCreated = true;
            }

            var newRow = dataTable.NewRow();
            foreach (var reportField in ReportModel.Fields)
            {
                newRow[reportField.Name] =
                    // ReSharper disable once PossibleNullReferenceException
                    type.GetProperty(reportField.SourceName).GetMethod.Invoke(entry, null);
            }

            dataTable.Rows.Add(newRow);
        }

        return dataTable;
    }
}