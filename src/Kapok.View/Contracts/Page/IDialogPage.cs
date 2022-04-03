namespace Kapok.View;

public interface IDialogPage : IPage
{
    bool? DialogResult { get; }
}