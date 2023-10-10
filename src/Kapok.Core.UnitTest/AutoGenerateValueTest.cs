using Kapok.Core.UnitTest.DataModel;
using Xunit;
using Kapok.Data.InMemory;
using Kapok.Entity;
using Kapok.Module;

namespace Kapok.Core.UnitTest;

public class AutoGenerateValueTest
{
    /// <summary>
    /// Tests that a property with attribute <see cref="AutoGenerateValueAttribute"/> automatically gets
    /// a new GUID when <c>dao.Init(entity)</c> is called and the auto generate value type is <see cref="AutoGenerateValueType.Identity"/>.
    /// </summary>
    [Fact]
    public void GuidIdentityProperty()
    {
        ModuleEngine.InitiateModule(typeof(ToDoModule));

        var dataDomain = new InMemoryDataDomain();

        var scope = dataDomain.CreateScope();

        var toDoItem = new ToDoItem();
        var dao = scope.GetDao<ToDoItem>();
        Assert.Equal(Guid.Empty, toDoItem.Id);
        dao.Init(toDoItem);
        Assert.NotEqual(Guid.Empty, toDoItem.Id);
    }
}