using System.Linq.Expressions;
using Kapok.Entity;

namespace Kapok.Core.UnitTest.DataModel;

public class ToDoList : EditableEntityBase, ITenantEntity
{
    static ToDoList()
    {
        RegisterModel<ToDoList>(entity =>
        {
            entity.AddManyToOneRelationship<ToDoItem>(nameof(Items))
                .HasPrincipalKey(nameof(ToDoItem.ToDoListId))
                .HasForeignKey(nameof(Id));
        });
    }

    private long _tenantId;
    private Guid _id;
    private string _name = string.Empty;

    public long TenantId
    {
        get => _tenantId;
        set => SetProperty(ref _tenantId, value);
    }

    [AutoGenerateValue(AutoGenerateValueType.Identity)]
    public Guid Id
    {
        get => _id;
        set => SetValidateProperty(ref _id, value);
    }

    public string Name
    {
        get => _name;
        set => SetValidateProperty(ref _name, value);
    }

    [AutoCalculate]
    public int NoOfItems { get; set; }

    public static Expression<Func<ToDoList, int>> AutoCalculateNoOfItems()
    {
        return e => e.Items == null ? 0 : e.Items.Count();
    }

    public  virtual ICollection<ToDoItem>? Items { get; set; }
}