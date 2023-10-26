using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kapok.Data.EntityFrameworkCore.Provider.PostgreSql;

public class TimestampValueConverter : ValueConverter<byte[], long>
{
    public TimestampValueConverter()
        : base(v => BitConverter.ToInt64(v, 0), v => BitConverter.GetBytes(v))
    {
    }
}