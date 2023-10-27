#if USE_JSON_LIBRARY_NEWTONSOFT
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json.Linq;

namespace Kapok.Data.EntityFrameworkCore.JsonConvert;

public class JObjectComparer : ValueComparer<JObject>
{
    public JObjectComparer()
        : base(
            (left, right) => JsonValueComparer<JObject>.IsJsonEquals(left, right),
            t => JsonValueComparer<JObject>.GetJsonHashCode(t),
            t => JsonValueComparer<JObject>.GetJsonSnapshot(t))
    {
    }
}
#endif