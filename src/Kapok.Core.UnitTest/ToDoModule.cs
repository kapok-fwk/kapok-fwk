using Kapok.Core.UnitTest.BusinessLayer;
using Kapok.Core.UnitTest.Contracts;
using Kapok.Core.UnitTest.DataModel;
using Kapok.Data;
using Kapok.Module;

namespace Kapok.Core.UnitTest;

public class ToDoModule : ModuleBase
{
    public ToDoModule() : base(typeof(ToDoModule).FullName ?? nameof(ToDoModule))
    {
    }

    public override void Initiate()
    {
        base.Initiate();

        DataDomain.RegisterEntity<ToDoItem, IToDoItemDao, ToDoItemDao>();
        DataDomain.RegisterEntity<ToDoList>();
    }
}