using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kapok.Data.EntityFrameworkCore.JsonConvert;

public class JsonObjectConverter : ValueConverter<JsonObject, string>
{
    // ReSharper disable once UnusedMember.Global
    public JsonObjectConverter()
        : this(default)
    {
    }

    public JsonObjectConverter(ConverterMappingHints? hints = default)
#pragma warning disable CS8603 // Possible null reference return.
        : base(value => JsonValueConverter<JsonObject>.ObjectToJsonString(value),
            value => JsonValueConverter<JsonObject>.JsonStringToObject(value),
            hints)
#pragma warning restore CS8603 // Possible null reference return.
    {
    }
}