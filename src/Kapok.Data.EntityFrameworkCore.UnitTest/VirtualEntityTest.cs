using System.Reflection;
using Kapok.BusinessLayer;
using Kapok.Data.EntityFrameworkCore.UnitTest.SampleModel;
using Kapok.Data.InMemory;
using Kapok.Module;
using Microsoft.Extensions.DependencyInjection;

namespace Kapok.Data.EntityFrameworkCore.UnitTest;

public class VirtualEntityTest : EFUnitTestBase
{
    static VirtualEntityTest()
    {
        ModuleEngine.InitiateModule(typeof(SampleModelModule));
    }

    protected override void ConfigureServices(IServiceCollection serviceCollection)
    {
        base.ConfigureServices(serviceCollection);

        serviceCollection.AddDataModelServices();
    }

    /// <summary>
    /// Checks if the right repository is taken by dependency injection.
    /// </summary>
    [Fact]
    public void RepositoryTakeRightTest()
    {
        var scope = DataDomain.CreateScope();

        var studentDao = scope.GetDao<Student>();
        var studentRepository = typeof(Dao<Student>)
            .GetField("Repository", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(studentDao);

        Assert.NotNull(studentRepository);
        Assert.Equal(typeof(EFCoreRepository<Student>), studentRepository.GetType());

        var studentVirtualDao = scope.GetDao<StudentVirtual>();
        var studentVirtualRepository = typeof(Dao<StudentVirtual>)
            .GetField("Repository", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(studentVirtualDao);

        Assert.NotNull(studentRepository);
        Assert.Equal(typeof(InMemoryRepository<StudentVirtual>), studentVirtualRepository.GetType());
    }
}