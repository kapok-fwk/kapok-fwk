using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.ChangeTracking;
#if USE_JSON_LIBRARY_NEWTONSOFT
using Newtonsoft.Json;
#endif

namespace Kapok.Data.EntityFrameworkCore;

internal class JsonValueComparer<T> : ValueComparer<T>
{
    public JsonValueComparer()
        : base(
            (left, right) => IsJsonEquals(left, right), 
            t => GetJsonHashCode(t), 
            t => GetJsonSnapshot(t))
    {
    }

    private static T GetJsonSnapshot(T instance)
    {
        if (instance is ICloneable cloneable)
            return (T)cloneable.Clone();

        if (instance is JsonNode jsonNode)
        {
            object? obj2 = JsonNode.Parse(jsonNode.ToJsonString());
            return (T)obj2;
        }
        
#if USE_JSON_LIBRARY_NEWTONSOFT
        var obj = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(instance), typeof(T));
        if (obj == null)
            throw new NotSupportedException("Deserialized JSON object is null");

        return (T)obj;
#else
        throw new NotImplementedException();
#endif
    }

    private static int GetJsonHashCode(T instance)
    {
        if (instance is IEquatable<T>)
            return instance.GetHashCode();

        if (instance is JsonNode jsonNode)
        {
            return jsonNode.ToJsonString().GetHashCode();
        }

#if USE_JSON_LIBRARY_NEWTONSOFT
        return JsonConvert.SerializeObject(instance).GetHashCode();
#else
        throw new NotImplementedException();
#endif
    }

    private static bool IsJsonEquals(T? left, T? right)
    {
        if (left is IEquatable<T> equatable)
            return equatable.Equals(right);

        if (left is JsonNode leftJsonNode &&
            right is JsonNode rightJsonNode)
        {
            return leftJsonNode.ToJsonString().Equals(
                rightJsonNode.ToJsonString()
            );
        }

#if USE_JSON_LIBRARY_NEWTONSOFT       
        return JsonConvert.SerializeObject(left).Equals(JsonConvert.SerializeObject(right));
#else
        throw new NotImplementedException();
#endif
    }
}