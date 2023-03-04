using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Kapok.Data.EntityFrameworkCore;

public static class PropertyBuilderExtensions
{
    public static PropertyBuilder<T> HasJsonValueConversion<T>(this PropertyBuilder<T> propertyBuilder)
        where T : class
    {
        propertyBuilder
            .HasConversion(new JsonValueConverter<T>())
            .Metadata.SetValueComparer(new JsonValueComparer<T>());

        return propertyBuilder;
    }

    public static PropertyBuilder HasJsonValueConversion(this PropertyBuilder propertyBuilder,
        Type propertyType)
    {
        var converterType = typeof(JsonValueConverter<>).MakeGenericType(propertyType);
        var comparerType = typeof(JsonValueComparer<>).MakeGenericType(propertyType);
            
        propertyBuilder
            .HasConversion((ValueConverter?) Activator.CreateInstance(converterType, default(ConverterMappingHints)))
            .Metadata.SetValueComparer((ValueComparer?) Activator.CreateInstance(comparerType));

        return propertyBuilder;
    }
}