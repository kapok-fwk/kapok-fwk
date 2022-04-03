namespace Kapok.View;

public interface IFilterSetView
{
    void Clear();
    void Reset();
    void Apply();

    event EventHandler ApplyFilter;
}