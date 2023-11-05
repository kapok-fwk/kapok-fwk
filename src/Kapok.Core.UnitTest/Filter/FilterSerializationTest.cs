using Kapok.BusinessLayer;
using Kapok.Core.UnitTest.DataModel;
using System.Linq.Expressions;
using System.Linq.Expressions.Bonsai.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace Kapok.Core.UnitTest.Filter;

public class FilterSerializationTest
{
    [Fact]
    public void BonsaiSerializationTest()
    {
        var filter = new PropertyStaticFilter(typeof(ToDoItem), nameof(ToDoItem.Description))
        {
            FilterValue = "ABC"
        };

        // serialize
        var obj = new ObjectSerializer();
        var s = new ExpressionSlimBonsaiSerializer(obj.GetJsonSerializer, obj.GetJsonDeserializer,
            BonsaiVersion.Default);
        var jsonObject = s.Serialize(filter.FilterExpression.ToExpressionSlim());
        var json = jsonObject.ToString();
        Assert.NotNull(json);

        // deserialize
        var jsonObject2 = Nuqleon.Json.Expressions.Expression.Parse(json);
        var expressionSlim = s.Deserialize(jsonObject2);
        var expression = expressionSlim.ToExpression();

        var filter2 = new Filter<ToDoItem>((Expression<Func<ToDoItem, bool>>)expression);
        Assert.NotNull(filter2.FilterExpression);

        // NOTE: a cast to a static property filter would be great here to test if the property and filter value is still the same
    }

    [Fact]
    public void IFilterJsonConverterTest()
    {
        var filter = new PropertyStaticFilter(typeof(ToDoItem), nameof(ToDoItem.Description))
        {
            FilterValue = "ABC"
        };

        var jsonString = JsonSerializer.Serialize((IFilter)filter, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters =
            {
                new IFilterJsonConverter()
            }
        });

        Assert.NotNull(jsonString);
        Assert.StartsWith("{\"Context", jsonString);

        var filter2 = JsonSerializer.Deserialize<IFilter>(jsonString, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            Converters =
            {
                new IFilterJsonConverter()
            }
        });
        Assert.NotNull(filter2);
        Assert.NotNull(filter2.FilterExpression);
    }
}