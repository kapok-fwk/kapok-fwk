using System.Linq.Expressions;
using System.Linq.Expressions.Bonsai.Serialization;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Kapok.BusinessLayer;

namespace Kapok.Core.UnitTest.Filter;

public class IFilterJsonConverter : JsonConverter<IFilter>
{
    public override IFilter? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var jsonElement = JsonElement.ParseValue(ref reader);

        var jsonObject = Nuqleon.Json.Expressions.Expression.Parse(jsonElement.ToString());

        var obj = new ObjectSerializer();
        var s = new ExpressionSlimBonsaiSerializer(obj.GetJsonSerializer, obj.GetJsonDeserializer,
            BonsaiVersion.Default);

        var expressionSlim = s.Deserialize(jsonObject);
        var expression = expressionSlim.ToExpression();

        // call 'return new Filter<TEntity>((Expression<Func<TEntity, bool>>)expression)' via reflection
        if (expressionSlim is not LambdaExpressionSlim lambdaExpression)
            return null;
        var entityType = lambdaExpression.DelegateType.ToType().GetGenericArguments()[0];

        var filterConstructor = typeof(Filter<>).MakeGenericType(entityType).GetConstructor(types: new Type[]
                { typeof(Expression<>).MakeGenericType(typeof(Func<,>).MakeGenericType(entityType, typeof(bool))) },
            bindingAttr: BindingFlags.Instance | BindingFlags.Public);

        return (IFilter)filterConstructor.Invoke(new object[] { expression });
    }

    public override void Write(Utf8JsonWriter writer, IFilter value, JsonSerializerOptions options)
    {
        var obj = new ObjectSerializer();
        var s = new ExpressionSlimBonsaiSerializer(obj.GetJsonSerializer, obj.GetJsonDeserializer,
            BonsaiVersion.Default);
        var jsonObject = s.Serialize(value.FilterExpression.ToExpressionSlim());
        var jsonNode = JsonNode.Parse(jsonObject.ToString());
        jsonNode.WriteTo(writer);
    }
}