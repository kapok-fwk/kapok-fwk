using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kapok.Data.EntityFrameworkCore.JsonConvert;

public class JsonArrayConverter : ValueConverter<JsonArray, string>
{
    // ReSharper disable once UnusedMember.Global
    public JsonArrayConverter()
        : this(default)
    {
    }

    public JsonArrayConverter(ConverterMappingHints? hints = default)
#pragma warning disable CS8603 // Possible null reference return.
        : base(value => JsonValueConverter<JsonArray>.ObjectToJsonString(value),
            value => JsonValueConverter<JsonArray>.JsonStringToObject(value),
            hints)
#pragma warning restore CS8603 // Possible null reference return.
    {
    }
}