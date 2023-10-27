using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Kapok.Data.EntityFrameworkCore.JsonConvert;

public class JsonArrayComparer : ValueComparer<JsonArray>
{
    public JsonArrayComparer()
        : base(
            (left, right) => JsonValueComparer<JsonArray>.IsJsonEquals(left, right),
            t => JsonValueComparer<JsonArray>.GetJsonHashCode(t),
            t => JsonValueComparer<JsonArray>.GetJsonSnapshot(t))
    {
    }
}