namespace Kapok.DataPort.Csv;

public class CsvDataPortTarget : IDataPortTableTarget
{
    private CsvHelper.LineSeparator _columnSeparator = CsvHelper.LineSeparator.Comma;
    private bool _headerIsWritten;
    private char _quoteChar;

    public virtual string Name => "CSV Data Port";

    public IReadOnlyList<DataPortColumn>? Schema { get; set; }

    public virtual bool WriteWithHeader { get; set; }

    /// <summary>
    /// By default <c>null</c> is parsed into an empty string.
    ///
    /// With this property you can define a custom value for <c>null</c> values.
    /// </summary>
    public string? NullValueString { get; set; }

    /// <summary>
    /// Defines the column separator.
    /// </summary>
    public virtual CsvHelper.LineSeparator ColumnSeparator
    {
        get => _columnSeparator;
        set
        {
            if (value == CsvHelper.LineSeparator.Unknown)
                throw new NotSupportedException(
                    $"The class {nameof(CsvDataPortTarget)} does not support line separator {value}");

            _columnSeparator = value;
        }
    }

    private char ColumnSeparatorAsChar
    {
        get
        {
            return ColumnSeparator switch
            {
                CsvHelper.LineSeparator.Tab => '\t',
                CsvHelper.LineSeparator.Semicolon => ';',
                CsvHelper.LineSeparator.Comma => ',',
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    public StreamWriter? StreamWriter { get; set; }

    /// <summary>
    /// Defines the quotation char. If not set, no quotation takes place.
    /// </summary>
    public char? QuoteChar { get; set; } = '"';

    /// <summary>
    /// If set all cells will be quoted.
    /// </summary>
    public bool QuoteAll { get; set; } = false;

    public virtual async void WriteHeader()
    {
        if (Schema == null) throw new InvalidOperationException($"You have to set memeber {nameof(Schema)} before calling this function");

        await WriteLineAsync(Schema.Select(column => column.Name ?? string.Empty));
    }

    public virtual string WriteCell(DataPortColumn column, object? value)
    {
        if (value == null)
            return NullValueString ?? string.Empty;

        // base converting on column type definition

        if (column.Type == typeof(string))
            return value.ToString() ?? string.Empty;

        // base converting on value type

        if (value is string stringValue)
            return stringValue;

        if (value is IFormattable formattableValue)
            return formattableValue.ToString() ?? string.Empty; // TODO use default formatting

        // fallback
        return value.ToString() ?? string.Empty;
    }

    public virtual async void Write(Dictionary<DataPortColumn, object?> rowValues)
    {
        if (Schema == null) throw new InvalidOperationException($"You have to set memeber {nameof(Schema)} before calling this function");

        if (WriteWithHeader && !_headerIsWritten)
        {
            WriteHeader();
            _headerIsWritten = true;
        }

        // make sure that we use the same column sorting as in the schema definition
        string[] row = new string[Schema.Count];
        for (int i = 0; i < Schema.Count; i++)
        {
            object? value = null;

            if (rowValues.ContainsKey(Schema[i]))
                value = rowValues[Schema[i]];

            row[i] = WriteCell(Schema[i], value);
        }

        await WriteLineAsync(row);
    }

    protected async Task WriteLineAsync(IEnumerable<string> rowCells)
    {
        if (StreamWriter == null) throw new InvalidOperationException($"You have to set memeber {nameof(StreamWriter)} before calling this function");

        IEnumerable<string> cells;

        if (QuoteAll)
        {
            if (!QuoteChar.HasValue)
                throw new NotSupportedException($"You have to provide {nameof(QuoteChar)} when {nameof(QuoteAll)} is true");

            cells = rowCells.Select(c => $"{QuoteChar}{c}{QuoteChar}");
        }
        else if (QuoteChar.HasValue)
        {
            cells = rowCells.Select(c => c.Contains(ColumnSeparatorAsChar) ? $"{QuoteChar}{c}{QuoteChar}" : c);
        }
        else
        {
            cells = rowCells;
        }

        await StreamWriter.WriteLineAsync(
            string.Join(ColumnSeparatorAsChar, cells)
        );
    }
}