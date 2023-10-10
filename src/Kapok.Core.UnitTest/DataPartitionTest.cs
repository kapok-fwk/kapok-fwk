using Kapok.Core.UnitTest.DataModel;
using Kapok.Data;
using Kapok.Data.InMemory;
using Kapok.Module;
using Xunit;

namespace Kapok.Core.UnitTest;

public class DataPartitionTest
{
    [Fact]
    public void TenantDataPartitionTest()
    {
        const int myTenantId = 12345;
        
        ModuleEngine.InitiateModule(typeof(ToDoModule));

        var dataDomain = new InMemoryDataDomain();
        dataDomain.RegisterDataPartition("Tenant", typeof(ITenantEntity), nameof(ITenantEntity.TenantId));

        dataDomain.DataPartitions["Tenant"].Value = myTenantId;

        {
            using var scope = dataDomain.CreateScope();

            // The tenant should be passed from the data domain to the data domain scope
            Assert.Equal(myTenantId, scope.DataPartitions["Tenant"].Value);
        
            var toDoListDao = scope.GetDao<ToDoList>();

            var newToDoList = toDoListDao.New();
            // the New() method should automatically fill the data partition with its current value.
            Assert.Equal(12345, newToDoList.TenantId);

            newToDoList.Id = new Guid("ab52ad77-ae0c-4ccf-b79c-cd445885ded9");
            newToDoList.Name = "My first ToDo list";

            toDoListDao.Create(newToDoList);
            scope.Save();
        }

        {
            using var scope = dataDomain.CreateScope();
            Assert.Equal(myTenantId, scope.DataPartitions["Tenant"].Value);
            var toDoListDao = scope.GetDao<ToDoList>();

            // check that we got the right to do list.
            var toDoList = toDoListDao.AsQueryable().First();
            Assert.NotNull(toDoList);
            Assert.Equal(new Guid("ab52ad77-ae0c-4ccf-b79c-cd445885ded9"), toDoList.Id);
        }

        // change the tenant id
        var otherTenantId = myTenantId + 1;
        dataDomain.DataPartitions["Tenant"].Value = otherTenantId;

        {
            using var scope = dataDomain.CreateScope();
            // now we should get the new tenant id which is not the same as the "myTenantId"
            Assert.NotEqual(myTenantId, scope.DataPartitions["Tenant"].Value);
            Assert.Equal(otherTenantId, scope.DataPartitions["Tenant"].Value);
            
            var toDoListDao = scope.GetDao<ToDoList>();
            
            // since we have a 'otherTenantId' where we never write to, we should not get any data here.
            Assert.Empty(toDoListDao.AsQueryable());
        }
    }
}