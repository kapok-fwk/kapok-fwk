using System.Linq.Expressions;
using Kapok.Entity;

namespace Kapok.Core.UnitTest.DataModel;

public class ToDoList : EditableEntityBase
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

    private Guid _id;
    private string _name = string.Empty;

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