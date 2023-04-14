using Kapok.BusinessLayer;
using Kapok.Core.UnitTest.DataModel;
using Kapok.Data;
using Xunit;

namespace Kapok.Core.UnitTest;

public class AutoCalculateTest
{
    /// <summary>
    /// Since version 0.1.5 an error
    /// ```
    ///     Field '__entry' defined on type 'AnonymousType' is not a field on the target object which
    ///     is of type 'Namespace.YourEntityType'.
    /// ```
    /// happened when using AutoCalculate on a query based on QueryTranslator<T>.
    /// This unit test is made to catch this exception and fix it finally.
    /// </summary>
    [Fact]
    public void PerformSimpleCollectionCount()
    {
        List<ToDoList> toDoLists = new()
        {
            new ToDoList
            {
                Id = new Guid(),
                Name = "ToDo List A"
            },

            new ToDoList
            {
                Id = new Guid(),
                Name = "ToDo List B",
                Items = new List<ToDoItem>()
            },
            new ToDoList
            {
                Id = Guid.Parse("c1e9e72c-a700-45d1-bf72-68d8f2f144b5"),
                Name = "ToDo List C",
                Items = new List<ToDoItem>
                {
                    new()
                    {
                        ToDoListId = Guid.Parse("c1e9e72c-a700-45d1-bf72-68d8f2f144b5"),
                        Description = "ToDo Item 1"
                    },
                    new()
                    {
                        ToDoListId = Guid.Parse("c1e9e72c-a700-45d1-bf72-68d8f2f144b5"),
                        Description = "ToDo Item 2"
                    }
                }
            }
        };

        var queryable = toDoLists.AsQueryable();

        var changeTracker = new ChangeTracker();

        var qt = new QueryTranslator<ToDoList>(queryable, changeTracker, new[]
        {
            typeof(ToDoList).GetProperty(nameof(ToDoList.Id))
        });

        var autoCalculateQuery = 
            (
                from l in qt
                select l
            )
            .AutoCalculate(new[] { nameof(ToDoList.NoOfItems) }, noTracking: false, nestedDataFilter: new Dictionary<string, object?>());

        // this executes the auto calculate query.
        var result = autoCalculateQuery.ToList();

        Assert.Equal(3, result.Count);
        Assert.Equal(0, result.First(l => l.Name == "ToDo List A").NoOfItems);
        Assert.Equal(0, result.First(l => l.Name == "ToDo List B").NoOfItems);
        Assert.Equal(2, result.First(l => l.Name == "ToDo List C").NoOfItems);
    }
}