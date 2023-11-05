using System.Diagnostics;
using System.Reflection;
using Kapok.BusinessLayer;
using Microsoft.Extensions.DependencyInjection;

namespace Kapok.Data;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add al registered data models to the <see cref="services"/>
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddDataModelServices(this IServiceCollection services)
    {
        var genericDaoType = typeof(IDao<>);
        var getDaoMethod1 = typeof(IDataDomainScope)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(m => m.Name == nameof(IDataDomainScope.GetDao) && m.GetParameters().Length == 0 && m.IsGenericMethod && m.GetGenericArguments().Length == 1);
        Debug.Assert(getDaoMethod1 != null);
        var getDaoMethod2 = typeof(IDataDomainScope)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(m => m.Name == nameof(IDataDomainScope.GetDao) && m.GetParameters().Length == 0 && m.IsGenericMethod && m.GetGenericArguments().Length == 2);
        Debug.Assert(getDaoMethod2 != null);

        foreach ((var entityType, var registeredEntityInfo) in DataDomain.Entities)
        {
            var daoType = genericDaoType.MakeGenericType(entityType);

            services.AddScoped(daoType,
                p => getDaoMethod1.MakeGenericMethod(entityType)
                .Invoke(p.GetRequiredService<IDataDomainScope>(), Array.Empty<object>()));
            if (registeredEntityInfo.ContractType != null)
            {
                services.AddScoped(registeredEntityInfo.ContractType,
                    p => getDaoMethod2.MakeGenericMethod(entityType, registeredEntityInfo.ContractType)
                        .Invoke(p.GetRequiredService<IDataDomainScope>(), Array.Empty<object>()));
            }
        }
        return services;
    }
}