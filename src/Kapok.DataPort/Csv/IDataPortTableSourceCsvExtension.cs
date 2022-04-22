namespace Kapok.DataPort.Csv;

// ReSharper disable once InconsistentNaming
public static class IDataPortTableSourceCsvExtension
{
    public static ITableDataPort ToCsvTargetDataPort(this IDataPortTableSource tableSource)
    {
        var tableDataPort = new TableDataPort<IDataPortTableSource, CsvDataPortTarget>(tableSource, new CsvDataPortTarget());

        var schema = tableDataPort.Source.ReadSchema();
        if (schema == null)
            throw new NotSupportedException("Could not read schema from table source");
        tableDataPort.Target.Schema = schema;

        tableDataPort.ColumnMappings = new List<TableDataPortMap>(schema.Count);
        foreach (var dataPortColumn in schema)
        {
            tableDataPort.ColumnMappings.Add(new TableDataPortMap
            {
                SourceColumn = dataPortColumn,
                TargetColumn = dataPortColumn
            });
        }

        return tableDataPort;
    }
}