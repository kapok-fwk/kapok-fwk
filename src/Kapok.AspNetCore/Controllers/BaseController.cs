using System.Reflection;
using Kapok.Data;
using Kapok.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kapok.AspNetCore.Controllers;

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
[Produces("application/json")]
//TODO[Authorize]
[Route("api/enty/[controller]")]
public abstract class EntityBaseController<T> : Controller
    where T : class
{
    /// <summary>
    /// 
    /// </summary>
    protected readonly ILogger Logger;
        
    /// <summary>
    /// 
    /// </summary>
    protected readonly IDataDomainScope DataDomainScope;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="serviceProvider"></param>
    protected EntityBaseController(ILogger<EntityBaseController<T>> logger, IServiceProvider serviceProvider)
    {
        Logger = logger;
        DataDomainScope = serviceProvider.GetRequiredService<IDataDomainScope>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected string GetRouteTemplate()
    {
        var attribute =
            typeof(T).GetCustomAttributes(typeof(GeneratedControllerAttribute), false).FirstOrDefault();

        if (attribute == null)
            return null;

        if (((GeneratedControllerAttribute) attribute).Route != null)
        {
            return ((GeneratedControllerAttribute) attribute).Route;
        }

        var routes = GetType().GetCustomAttributes(typeof(RouteAttribute));
        var route = routes.FirstOrDefault();

        return ((RouteAttribute)route)?.Template.Replace("[controller]", typeof(T).Name);
    }
}