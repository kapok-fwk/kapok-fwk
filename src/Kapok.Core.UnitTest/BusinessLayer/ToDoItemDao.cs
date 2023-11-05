using Kapok.BusinessLayer;
using Kapok.Core.UnitTest.Contracts;
using Kapok.Core.UnitTest.DataModel;
using Kapok.Data;

namespace Kapok.Core.UnitTest.BusinessLayer;

public class ToDoItemDao : Dao<ToDoItem>, IToDoItemDao
{
    public ToDoItemDao(IDataDomainScope dataDomainScope, IRepository<ToDoItem> repository, bool isReadOnly = false) : base(dataDomainScope, repository, isReadOnly)
    {
    }
}