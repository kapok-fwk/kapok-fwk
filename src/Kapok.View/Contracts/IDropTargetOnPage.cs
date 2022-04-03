namespace Kapok.View;

public interface IDropTargetOnPage : IPage
{
    bool CanDropFile(string[] filenames);
    void DropFile(string[] filenames);
}