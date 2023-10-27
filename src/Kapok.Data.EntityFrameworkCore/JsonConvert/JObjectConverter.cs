#if USE_JSON_LIBRARY_NEWTONSOFT
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json.Linq;

namespace Kapok.Data.EntityFrameworkCore.JsonConvert;

public class JObjectConverter : ValueConverter<JObject, string>
{
    public JObjectConverter(ConverterMappingHints? hints = default)
#pragma warning disable CS8603 // Possible null reference return.
        : base(value => JsonValueConverter<JObject>.ObjectToJsonString(value),
            value => JsonValueConverter<JObject>.JsonStringToObject(value),
            hints)
#pragma warning restore CS8603 // Possible null reference return.
    {
    }
}
#endif