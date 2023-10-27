#if USE_JSON_LIBRARY_NEWTONSOFT

using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json.Linq;

namespace Kapok.Data.EntityFrameworkCore.JsonConvert;

public class JArrayComparer : ValueComparer<JArray>
{
    public JArrayComparer()
        : base(
            (left, right) => JsonValueComparer<JArray>.IsJsonEquals(left, right),
            t => JsonValueComparer<JArray>.GetJsonHashCode(t),
            t => JsonValueComparer<JArray>.GetJsonSnapshot(t))
    {
    }
}
#endif