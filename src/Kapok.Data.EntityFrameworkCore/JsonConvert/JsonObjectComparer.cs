using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Kapok.Data.EntityFrameworkCore.JsonConvert;

public class JsonObjectComparer : ValueComparer<JsonObject>
{
    public JsonObjectComparer()
        : base(
            (left, right) => JsonValueComparer<JsonObject>.IsJsonEquals(left, right),
            t => JsonValueComparer<JsonObject>.GetJsonHashCode(t),
            t => JsonValueComparer<JsonObject>.GetJsonSnapshot(t))
    {
    }
}