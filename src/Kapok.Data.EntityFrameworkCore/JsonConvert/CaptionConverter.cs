using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kapok.Data.EntityFrameworkCore.JsonConvert;

public class CaptionConverter : ValueConverter<Caption, string>
{
    // ReSharper disable once UnusedMember.Global
    public CaptionConverter()
        : this(default)
    {
    }

    public CaptionConverter(ConverterMappingHints? hints = default)
#pragma warning disable CS8603 // Possible null reference return.
        : base(value => JsonValueConverter<Caption>.ObjectToJsonString(value),
            value => JsonValueConverter<Caption>.JsonStringToObject(value),
            hints)
#pragma warning restore CS8603 // Possible null reference return.
    {
    }
}