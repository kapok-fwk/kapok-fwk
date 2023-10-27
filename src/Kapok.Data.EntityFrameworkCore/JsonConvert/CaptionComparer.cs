using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Kapok.Data.EntityFrameworkCore.JsonConvert;

public class CaptionComparer : ValueComparer<Caption>
{
    public CaptionComparer()
        : base(
            (left, right) => JsonValueComparer<Caption>.IsJsonEquals(left, right),
            t => JsonValueComparer<Caption>.GetJsonHashCode(t),
            t => JsonValueComparer<Caption>.GetJsonSnapshot(t))
    {
    }
}