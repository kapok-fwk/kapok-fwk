using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;

namespace Kapok.Data.EntityFrameworkCore;

public class JsonValueConverter<T> : ValueConverter<T, string>
    where T : class
{
    public JsonValueConverter(ConverterMappingHints? hints = default) :
#pragma warning disable 8603
        base(value => JsonConvert.SerializeObject(value),
            value => string.IsNullOrWhiteSpace(value) ? null : JsonConvert.DeserializeObject<T>(value),
            hints)
#pragma warning restore 8603
    {
    }
}