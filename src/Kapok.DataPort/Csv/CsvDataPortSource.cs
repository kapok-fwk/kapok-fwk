using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Kapok.DataPort.Csv;

public class CsvDataPortSource : IDataPortTableSource
{
    public virtual string Name => "CSV Data Port";

    public virtual bool HasHeader { get; set; }

    public virtual CsvHelper.LineSeparator ColumnSeparator { get; set; }

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
                Type = typeof(string)
            });
        }

        Schema = schema;

        return schema;
    }

    public virtual object[]? ReadNextRow()
    {
        if (StreamReader == null)
            throw new NotSupportedException($"You need to set {nameof(StreamReader)} before you call {nameof(ReadNextRow)}");

        var newLine = StreamReader.ReadLine();

        if (newLine == null)
            return null;

        return CsvHelper.DictionaryOfLineSeparatorAndItsFunc[ColumnSeparator].Invoke(newLine).Cast<object>().ToArray();
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