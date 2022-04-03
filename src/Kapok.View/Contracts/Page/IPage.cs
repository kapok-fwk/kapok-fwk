namespace Kapok.View;

public interface IPage
{
    IViewDomain ViewDomain { get; }

    string? Title { get; set; }

    void Show();
    bool? ShowDialog();
    bool? ShowDialog(IPage owner);

    // actions
    IAction CloseAction { get; }

    // events
    event EventHandler Closed;
}