namespace Kapok.View;

public interface IAction
{
    string Name { get; }

    string? Image { get; }

    bool? ImageIsBig { get; }

    /// <summary>
    /// If the action is visible in the UI.
    /// </summary>
    bool IsVisible { get; set; }

    event EventHandler? CanExecuteChanged;

    bool CanExecute();

    void Execute();
}

public interface IAction<in T>
{
    string Name { get; }

    string? Image { get; }

    bool? ImageIsBig { get; }

    /// <summary>
    /// If the action is visible in the UI.
    /// </summary>
    bool IsVisible { get; set; }

    event EventHandler? CanExecuteChanged;

    bool CanExecute(T? arg);

    void Execute(T? arg);
}