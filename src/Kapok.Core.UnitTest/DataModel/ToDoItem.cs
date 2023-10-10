using Kapok.Entity;

namespace Kapok.Core.UnitTest.DataModel;

public class ToDoItem : EditableEntityBase, ITenantEntity
{
    private long _tenantId;
    private Guid _id;
    private Guid? _toDoListId;
    private string _description = string.Empty;

    public long TenantId
    {
        get => _tenantId;
        set => SetProperty(ref _tenantId, value);
    }

    [AutoGenerateValue(AutoGenerateValueType.Identity)]
    public Guid Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

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