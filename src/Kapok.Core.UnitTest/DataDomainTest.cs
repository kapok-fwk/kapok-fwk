using Kapok.BusinessLayer;
using Kapok.Core.UnitTest.Contracts;
using Kapok.Core.UnitTest.DataModel;
using Kapok.Data;
using Kapok.Data.InMemory;
using Kapok.Module;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Kapok.Core.UnitTest;

public class DataDomainTest
{
    [Fact]
    public void ConstructNewEntityServiceTest()
    {
        ModuleEngine.InitiateModule(typeof(ToDoModule));

        var dataDomain = new InMemoryDataDomain();
        using var scope = dataDomain.CreateScope();

        var entityService = scope.GetEntityService<ToDoItem>();
        Assert.NotNull(entityService);
    }

    /// <summary>
    /// A test to check if adding all data models to a service collection works.
    /// </summary>
    [Fact]
    public void ServiceProviderPreloadedTest()
    {
        ModuleEngine.InitiateModule(typeof(ToDoModule));

        var dataDomain = new InMemoryDataDomain();

        var services = new ServiceCollection();
        services.AddSingleton<IDataDomain>(dataDomain);
        services.AddScoped<IDataDomainScope>(s => s.GetService<IDataDomain>()!.CreateScope());
        services.AddDataModelServices();
        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.GetRequiredService<IEntityService<ToDoItem>>();
        serviceProvider.GetRequiredService<IEntityService<ToDoList>>();
        serviceProvider.GetRequiredService<IToDoItemService>();
    }
}