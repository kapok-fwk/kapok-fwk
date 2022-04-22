using System.Globalization;

namespace Kapok.DataPort.Csv;

public class CsvDataPortTarget : IDataPortTableTarget
{
    public virtual string Name => "CSV Data Port";

    public IReadOnlyList<DataPortColumn>? Schema { get; set; }

    public virtual bool WriteWithHeader { get; set; }

    /// <summary>
    /// Defines the column separator.
    /// </summary>
    private CsvHelper.LineSeparator _columnSeparator = CsvHelper.LineSeparator.Comma;

    /// <summary>
    /// By default <c>null</c> is parsed into an empty string.
    ///
    /// With this property you can define a custom value for <c>null</c> values.
    /// </summary>
    public string? NullValueString { get; set; }

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

    private bool _headerIsWritten;

    public virtual void WriteHeader()
    {
        if (StreamWriter == null) throw new InvalidOperationException($"You have to set memeber {nameof(StreamWriter)} before calling this function");
        if (Schema == null) throw new InvalidOperationException($"You have to set memeber {nameof(Schema)} before calling this function");

        StreamWriter.WriteLine(
            string.Join(ColumnSeparatorAsChar, Schema.Select(column => column.Name))
        );
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

    public virtual void Write(Dictionary<DataPortColumn, object?> rowValues)
    {
        if (StreamWriter == null) throw new InvalidOperationException($"You have to set memeber {nameof(StreamWriter)} before calling this function");
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

        StreamWriter.WriteLine(
            string.Join(ColumnSeparatorAsChar, row)
        );
    }
}