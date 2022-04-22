using System.Collections;

namespace Kapok.DataPort.Csv;

public class CsvDataPortSource : IDataPortTableSource
{
    public virtual string Name => "CSV Data Port";

    public virtual bool HasHeader { get; set; }

    public virtual CsvHelper.LineSeparator ColumnSeparator { get; set; }

    /// <summary>
    /// Defines which string value shall be converted into <c>null</c> when reading.
    ///
    /// When this value is null, no transformation takes place. Empty cells will then be read as an empty string.
    /// </summary>
    public string? NullValueString { get; set; }

    public StreamReader? StreamReader { get; set; }

    public List<DataPortColumn>? Schema { get; private set; }

    public virtual List<DataPortColumn>? ReadSchema()
    {
        if (!HasHeader)
            return null;

        if (StreamReader == null)
            throw new NotSupportedException($"It is not supported to call {nameof(ReadSchema)} before calling {nameof(StreamReader)}");

        if (StreamReader.BaseStream.Position != 0)
            throw new NotSupportedException("Someone already started reading the stream, the schema header can not be read anymore");

        var headerLine = StreamReader.ReadLine();

        // guess the csv column separator if not set
        if (ColumnSeparator == CsvHelper.LineSeparator.Unknown)
            ColumnSeparator = CsvHelper.GuessCsvSeparator(headerLine);

        var columns = CsvHelper.DictionaryOfLineSeparatorAndItsFunc[ColumnSeparator].Invoke(headerLine);

        var schema = new List<DataPortColumn>(columns.Length);

        foreach (var column in columns)
        {
            schema.Add(new DataPortColumn
            {
                Name = column,
                Type = typeof(string),
                Required = false
            });
        }

        Schema = schema;

        return schema;
    }

    public virtual object?[]? ReadNextRow()
    {
        if (StreamReader == null)
            throw new NotSupportedException($"You need to set {nameof(StreamReader)} before you call {nameof(ReadNextRow)}");

        var newLine = StreamReader.ReadLine();

        if (newLine == null)
            return null;

        string[] cellStringValues = CsvHelper.DictionaryOfLineSeparatorAndItsFunc[ColumnSeparator].Invoke(newLine);
        object?[] cellObjectValues = new object?[cellStringValues.Length];

        for (int i = 0; i < cellStringValues.Length; i++)
        {
            var cellString = cellStringValues[i];

            if (NullValueString == null)
            {
                cellObjectValues[i] = cellString;
            }
            else
            {
                cellObjectValues[i] = cellString == NullValueString ? null : cellString;
            }
        }

        return cellObjectValues;
    }

    public IEnumerator<object[]> GetEnumerator()
    {
        if (HasHeader && Schema == null)
            ReadSchema();

        while (true)
        {
            object[]? row = ReadNextRow();

            if (row == null)
                yield break;

            yield return row;
        }
    }

    #region IDataPortTableSource

    bool IDataPortTableSource.HasSchema => HasHeader;

    #endregion

    #region IEnumerable

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion
}