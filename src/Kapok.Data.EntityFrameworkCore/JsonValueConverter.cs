using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
#if USE_JSON_LIBRARY_NEWTONSOFT
using Newtonsoft.Json;
#endif

namespace Kapok.Data.EntityFrameworkCore;

public class JsonValueConverter<T> : ValueConverter<T, string>
    where T : class
{
    public JsonValueConverter(ConverterMappingHints? hints = default) :
#pragma warning disable 8603
        base(value => ObjectToJsonString(value),
            value => JsonStringToObject(value),
            hints)
#pragma warning restore 8603
    {
    }

    private static string? ObjectToJsonString(T? value)
    {
        if (value == null)
            return null;
        
        if (value is JsonNode jsonNode)
            return jsonNode.ToJsonString();

#if USE_JSON_LIBRARY_NEWTONSOFT
        return JsonConvert.SerializeObject(value);
#else
        throw new NotImplementedException();
#endif
    }

    private static T? JsonStringToObject(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (typeof(T) == typeof(JsonNode) ||
            typeof(T).IsSubclassOf(typeof(JsonNode)))
        {
            return JsonNode.Parse(value) as T;
        }

#if USE_JSON_LIBRARY_NEWTONSOFT
        return JsonConvert.DeserializeObject<T>(value);
#else
        throw new NotImplementedException();
#endif
    }
}