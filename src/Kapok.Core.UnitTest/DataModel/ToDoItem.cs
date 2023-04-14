using Kapok.Entity;

namespace Kapok.Core.UnitTest.DataModel;

public class ToDoItem : EditableEntityBase
{
    private Guid? _toDoListId;
    private string _description = string.Empty;

    public Guid? ToDoListId
    {
        get => _toDoListId;
        set => SetProperty(ref _toDoListId, value);
    }

    public string Description
    {
        get => _description;
        set => SetValidateProperty(ref _description, value);
    }
}