namespace Kapok.View;

public interface IInteractivePage : IPage
{
    //public IReadOnlyDictionary<string, object> Menu { get; }

    ICollection<IDetailPage> DetailPages { get; }
}