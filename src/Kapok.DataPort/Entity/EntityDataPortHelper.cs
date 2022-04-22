using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Kapok.DataPort.Entity;

internal class EntityDataPortHelper
{
    public static List<DataPortColumn> ReadSchema(Type entityType)
    {
        var properties = entityType.GetProperties(BindingFlags.Instance | BindingFlags.Public |
                                                    BindingFlags.GetProperty | BindingFlags.SetProperty);

        var schema = new List<DataPortColumn>();

        foreach (var propertyInfo in properties)
        {
            if (propertyInfo.GetCustomAttribute<NotMappedAttribute>() != null)
                continue; // we do not allow to write into not mapped attributes

            var browsableAttribute = propertyInfo.GetCustomAttribute<BrowsableAttribute>();
            if (browsableAttribute != null && browsableAttribute.Browsable == false)
                continue; // we do not allow to write into not browsable properties

            var newColumn = new DataPortPropertyColumn(propertyInfo);
                
            schema.Add(newColumn);
        }

        return schema;
    }
}