using System.Text;
using Kapok.Data;

namespace Kapok.Entity;
// see also: https://www.strathweb.com/2018/04/generic-and-dynamically-generated-controllers-in-asp-net-core-mvc/

public enum BaseControllerType
{
    /// <summary>
    /// A controller with no data operation functionality.
    /// </summary>
    BaseController = 1,

    /// <summary>
    /// A controller which only allows reads.
    /// </summary>
    ReadonlyController = 2,
        
    /// <summary>
    /// A controller which allows typical CRUD operations.
    /// </summary>
    CrudController = 3
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class GeneratedControllerAttribute : Attribute
{
    public GeneratedControllerAttribute()
    {
    }

    public GeneratedControllerAttribute(string controllerName)
    {
        ControllerName = controllerName;
    }

    public string Route { get; set; }

    public string ControllerName { get; }

    public BaseControllerType ControllerType { get; set; } = BaseControllerType.CrudController;

    public string BuildDefaultRoute(Type classType)
    {
        if (ControllerName == string.Empty)
        {
            throw new NotSupportedException($"The method {nameof(BuildDefaultRoute)} requires that the property {nameof(ControllerName)} is set.");
        }

        // NOTE: the route mask could be retrieved from the route attribute of the basis class
        var routeMaskBuilder = new StringBuilder();
        routeMaskBuilder.Append("api/v1/");
        foreach (var dataPartitions in DataDomain.Default?.DataPartitions.Values ?? new List<DataPartition>())
        {
            routeMaskBuilder.Append($"{{{dataPartitions.PartitionProperty.Name}}}");
        }
        routeMaskBuilder.Append("[controller]");

        var routeMask = routeMaskBuilder.ToString();

        switch (ControllerType)
        {
            case BaseControllerType.BaseController:
            case BaseControllerType.ReadonlyController:
            case BaseControllerType.CrudController:
                return routeMask.Replace("[controller]", ControllerName);
            default:
                throw new ArgumentOutOfRangeException(nameof(ControllerType), ControllerType, null);
        }
    }
}