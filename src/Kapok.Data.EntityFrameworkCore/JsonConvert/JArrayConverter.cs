#if USE_JSON_LIBRARY_NEWTONSOFT

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json.Linq;

namespace Kapok.Data.EntityFrameworkCore.JsonConvert;

public class JArrayConverter : ValueConverter<JArray, string>
{
    // ReSharper disable once UnusedMember.Global
    public JArrayConverter()
        : this(default)
    {
    }

    public JArrayConverter(ConverterMappingHints? hints = default)
#pragma warning disable CS8603 // Possible null reference return.
        : base(value => JsonValueConverter<JArray>.ObjectToJsonString(value),
            value => JsonValueConverter<JArray>.JsonStringToObject(value),
            hints)
#pragma warning restore CS8603 // Possible null reference return.
    {
    }
}
#endif