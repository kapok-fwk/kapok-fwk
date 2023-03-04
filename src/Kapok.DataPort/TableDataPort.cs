using System.Globalization;

namespace Kapok.DataPort;

public class TableDataPort : ITableDataPort
{
    public TableDataPort(IDataPortTableSource source, IDataPortTableTarget target)
    {
        Source = source;
        Target = target;
    }

    public IDataPortTableSource Source { get; }
    public IDataPortTableTarget Target { get; }

    public List<TableDataPortMap>? ColumnMappings { get; set; }

    public void Execute()
    {
        if (!Source.HasSchema)
            throw new NotSupportedException($"The source does not provide any schema, this source can not be used with the {nameof(TableDataPort)} data port.");
        if (ColumnMappings == null)
            return; // nothing mapped --> so, nothing to write

        var sourceSchema = Source.ReadSchema();

        if (sourceSchema == null)
            throw new Exception($"Could not read schema from source {Source.GetType()}");

        // stores the index in the source index schema, where the position in the array is the column mapping index;
        // when the column is not read from source, the value is -1
        var sourceIndexMap = new int[ColumnMappings.Count];

        for (var index = 0; index < ColumnMappings.Count; index++)
        {
            var map = ColumnMappings[index];

            if (map.TargetColumn == null) // NOTE: we ignore mappings where no target column is mapped
                continue;

            if (map.SourceColumn == null)
            {
                if (map.TargetColumn.Required)
                    throw new NotSupportedException($"The target column with name {map.TargetColumn.Name} is not mapped, but is required.");

                sourceIndexMap[index] = -1;
            }
            else
            {
                var sourceColumnIndex = sourceSchema.FindIndex(c => c.Name == map.SourceColumn.Name);
                if (sourceColumnIndex == -1)
                    throw new NotSupportedException($"The source column with name {map.SourceColumn.Name} could not be found in the source schema.");

                sourceIndexMap[index] = sourceColumnIndex;
            }
        }

        int lineNum = 1;
        foreach (var line in Source)
        {
            var row = new Dictionary<DataPortColumn, object?>(ColumnMappings.Count);

            for (var index = 0; index < ColumnMappings.Count; index++)
            {
                var map = ColumnMappings[index];

                if (map.TargetColumn == null) // NOTE: we ignore mappings where no target column is mapped
                    continue;

                var sourceColumnIndex = sourceIndexMap[index];

                if (sourceColumnIndex >= line.Length)
                {
                    if (map.TargetColumn.Required)
                        throw new NotSupportedException($"The target column {map.TargetColumn.Name} is required but is not filled in source line {lineNum}.");
                }
                else if (sourceColumnIndex >= 0)
                {
                    var sourceValue = line[sourceColumnIndex];
                    var targetValue = MapValue(map, sourceValue, lineNum);
                    row.Add(map.TargetColumn, targetValue);
                }
                else
                {
                    // set the default value
                    var targetValue = MapValue(map, null, lineNum);
                    row.Add(map.TargetColumn, targetValue);
                }
            }

            Target.Write(row);

            lineNum++;
        }
    }

    protected virtual object? MapValue(TableDataPortMap map, object? sourceValue, int lineNum)
    {
        if (map.CustomMapValue != null)
            return map.CustomMapValue(sourceValue);

        if (map.TargetColumn == null)
            return null;

        if (map.SourceColumn == null)
        {
            // return the default value of the target type
            return map.TargetColumn.Type.GetTypeDefault();
        }

        if (map.SourceColumn.Type == typeof(string))
        {
            if (map.TargetColumn.Type == typeof(DateTime))
            {
                if (!DateTime.TryParseExact((string?) sourceValue, map.SourceFormats, map.SourceCultureInfo,
                        DateTimeStyles.None, out var sourceDateTime))
                {
                    throw new NotSupportedException($"Could not read DateTime from string '{(string?)sourceValue}' in line {lineNum}");
                }

                return sourceDateTime;
            }

            if (map.TargetColumn.Type == typeof(int))
            {
                if (!int.TryParse((string?) sourceValue, NumberStyles.Integer | NumberStyles.AllowThousands, map.SourceCultureInfo,
                        out var sourceInt))
                {
                    throw new NotSupportedException($"Could not read int from string '{(string?)sourceValue}' in line {lineNum}");
                }

                return sourceInt;
            }

            if (map.TargetColumn.Type == typeof(decimal))
            {
                if (!decimal.TryParse((string?) sourceValue, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, map.SourceCultureInfo,
                        out var sourceDecimal))
                {
                    throw new NotSupportedException($"Could not read decimal from string '{(string?)sourceValue}' in line {lineNum}");
                }

                return sourceDecimal;
            }
        }

        if (map.TargetColumn.Type == typeof(string))
        {
            if (sourceValue == null)
            {
                if (map.TargetColumn.Required)
                    return string.Empty;

                return null;
            }
                
            if (typeof(IFormattable).IsAssignableFrom(map.SourceColumn.Type))
            {
                return ((IFormattable) sourceValue).ToString(map.TargetFormat, map.TargetCultureInfo);
            }

            return sourceValue.ToString();
        }

        throw new NotSupportedException($"Not supported mapping from type {map.SourceColumn.Type?.FullName} to type {map.TargetColumn.Type?.FullName}");
    }

    #region IDataPort

    IDataPortSource IDataPort.Source => this.Source;
    IDataPortTarget IDataPort.Target => this.Target;

    #endregion
}

public class TableDataPort<TDataPortSource, TDataPortTarget> : TableDataPort
    where TDataPortSource : IDataPortTableSource
    where TDataPortTarget : IDataPortTableTarget
{
    public TableDataPort(TDataPortSource source, TDataPortTarget target)
        : base(source, target)
    {
    }

    public new TDataPortSource Source => (TDataPortSource) base.Source;

    public new TDataPortTarget Target => (TDataPortTarget) base.Target;
}