using System.Linq.Expressions;
using System.Reflection;
using Kapok.BusinessLayer;
using Xunit;

namespace Kapok.Core.UnitTest;

public class FilterExpressionModifierTest
{
    private class Entity
    {
        public int? Partition { get; set; }
        public string? DataArea { get; set; }
    }

    private static readonly PropertyInfo PartitionPropertyInfo =
#pragma warning disable CS8601
        typeof(Entity).GetProperty(nameof(Entity.Partition));
#pragma warning restore CS8601

    private static readonly PropertyInfo DataAreaPropertyInfo =
#pragma warning disable CS8601
        typeof(Entity).GetProperty(nameof(Entity.DataArea));
#pragma warning restore CS8601

    private IList<Entity> BuildList()
    {
        List<Entity> list = new List<Entity>
        {
            new() {Partition = null, DataArea = null},
            new() {Partition = 1, DataArea = "A"},
            new() {Partition = 1, DataArea = "B"},
            new() {Partition = 2, DataArea = "B"},
            new() {Partition = 2, DataArea = "B"},
            new() {Partition = 2, DataArea = "B"}
        };
        return list;
    }

    [Fact]
    public void GetPropertyIntNullableTest()
    {
        Expression<Func<Entity, bool>> whereExpression = entity => entity.Partition == 1;

        var modifier =
            new FilterExpressionModifier(FilterExpressionModifierAction.GetFilterValue, typeof(Entity),PartitionPropertyInfo);
        modifier.Visit(whereExpression);

        Assert.True(modifier.FoundFilter);
        Assert.Equal(1, modifier.ParameterValue);
    }

    [Fact]
    public void GetPropertyStringTest()
    {
        Expression<Func<Entity, bool>> whereExpression = entity => entity.DataArea == "A";

        var modifier =
            new FilterExpressionModifier(FilterExpressionModifierAction.GetFilterValue, typeof(Entity),DataAreaPropertyInfo);
        modifier.Visit(whereExpression);

        Assert.True(modifier.FoundFilter);
        Assert.Equal("A", modifier.ParameterValue);
    }

    [Fact]
    public void SetPropertyIntNullableTest()
    {
        var list = BuildList();

        Expression<Func<Entity, bool>>? whereExpression = entity => true;
            
        Assert.Equal(6, list.Where(whereExpression.Compile()).Count());

            
        var modifier = new FilterExpressionModifier(
            FilterExpressionModifierAction.SetFilterValue,
            typeof(Entity),
            PartitionPropertyInfo);
        modifier.ParameterValue = 2;

        whereExpression = (Expression<Func<Entity, bool>>?)modifier.Visit(whereExpression);
        Assert.NotNull(whereExpression);

        Assert.False(modifier.FoundFilter);
#pragma warning disable CS8602
        Assert.Equal(3, list.Where(whereExpression.Compile()).Count());
#pragma warning restore CS8602
    }

    [Fact]
    public void SetPropertyStringTest()
    {
        var list = BuildList();

        Expression<Func<Entity, bool>>? whereExpression = entity => true;
            
        Assert.Equal(6, list.Where(whereExpression.Compile()).Count());

        var modifier = new FilterExpressionModifier(
            FilterExpressionModifierAction.SetFilterValue,
            typeof(Entity),
            DataAreaPropertyInfo);
        modifier.ParameterValue = "B";

        whereExpression = (Expression<Func<Entity, bool>>?)modifier.Visit(whereExpression);
        Assert.NotNull(whereExpression);

        Assert.False(modifier.FoundFilter);
#pragma warning disable CS8602
        Assert.Equal(4, list.Where(whereExpression.Compile()).Count());
#pragma warning restore CS8602
    }
        
    [Fact]
    public void ChangeSetPropertyIntNullableTest()
    {
        var list = BuildList();

        Expression<Func<Entity, bool>>? whereExpression = entity => entity.Partition == 1;
            
        Assert.Equal(2, list.Where(whereExpression.Compile()).Count());

            
        var modifier = new FilterExpressionModifier(
            FilterExpressionModifierAction.SetFilterValue,
            typeof(Entity),
            PartitionPropertyInfo);
        modifier.ParameterValue = 2;

        whereExpression = (Expression<Func<Entity, bool>>?)modifier.Visit(whereExpression);
        Assert.NotNull(whereExpression);

        Assert.True(modifier.FoundFilter);
#pragma warning disable CS8602
        Assert.Equal(3, list.Where(whereExpression.Compile()).Count());
#pragma warning restore CS8602
    }

    [Fact]
    public void ChangeSetPropertyStringTest()
    {
        var list = BuildList();

        Expression<Func<Entity, bool>>? whereExpression = entity => entity.DataArea == "A";

        Assert.Single(list.Where(whereExpression.Compile()));

        var modifier = new FilterExpressionModifier(
            FilterExpressionModifierAction.SetFilterValue,
            typeof(Entity),
            DataAreaPropertyInfo);
        modifier.ParameterValue = "B";

        whereExpression = (Expression<Func<Entity, bool>>?)modifier.Visit(whereExpression);
        Assert.NotNull(whereExpression);

        Assert.True(modifier.FoundFilter);
#pragma warning disable CS8602
        Assert.Equal(4, list.Where(whereExpression.Compile()).Count());
#pragma warning restore CS8602
    }

    [Fact]
    public void RemovePropertyIntNullableTest()
    {
        var list = BuildList();

        Expression<Func<Entity, bool>>? whereExpression = entity => entity.Partition == 1;
            
        Assert.Equal(2, list.Where(whereExpression.Compile()).Count());

        var modifier = new FilterExpressionModifier(
            FilterExpressionModifierAction.RemoveFilter,
            typeof(Entity),
            PartitionPropertyInfo);

        whereExpression = (Expression<Func<Entity, bool>>?)modifier.Visit(whereExpression);
        Assert.NotNull(whereExpression);

        Assert.True(modifier.FoundFilter);
#pragma warning disable CS8602
        Assert.Equal(6, list.Where(whereExpression.Compile()).Count());
#pragma warning restore CS8602
    }

    [Fact]
    public void RemovePropertyStringTest()
    {
        var list = BuildList();

        Expression<Func<Entity, bool>>? whereExpression = entity => entity.DataArea == "A";
            
        Assert.Single(list.Where(whereExpression.Compile()));

        var modifier = new FilterExpressionModifier(
            FilterExpressionModifierAction.RemoveFilter,
            typeof(Entity),
            DataAreaPropertyInfo);

        whereExpression = (Expression<Func<Entity, bool>>?)modifier.Visit(whereExpression);
        Assert.NotNull(whereExpression);

        Assert.True(modifier.FoundFilter);
#pragma warning disable CS8602
        Assert.Equal(6, list.Where(whereExpression.Compile()).Count());
#pragma warning restore CS8602
    }
}