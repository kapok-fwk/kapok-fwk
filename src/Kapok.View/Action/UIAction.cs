using System.Diagnostics;
using Kapok.BusinessLayer;
using Kapok.Entity;
using NLog;

namespace Kapok.View;

// ReSharper disable once InconsistentNaming
public class UIAction : BindableObjectBase, IAction
{
    private string? _image;
    private bool _isVisible;
    internal Action ExecuteFunc;
    private readonly Func<bool>? _canExecuteFunc;
    private bool? _imageIsBig;

    protected static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

    public UIAction(string name, Action execute, Func<bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);
        Name = name;
        _isVisible = true;
        ExecuteFunc = execute;
        _canExecuteFunc = canExecute;
    }

    public string Name { get; }

    public string? Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }

    public bool? ImageIsBig
    {
        get => _imageIsBig;
        set => SetProperty(ref _imageIsBig, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool CanExecute()
    {
        try
        {
            return _canExecuteFunc?.Invoke() ?? true;
        }
        catch (BusinessLayerErrorException)
        {
        }
        catch (Exception e)
        {
            Debugger.Break();
            Logger.Error(e, "CanExecute action {actionName} failed", Name);
        }

        return false;
    }

    public void Execute()
    {
        Logger.Debug("Call action {actionName}", Name);
        try
        {
            ExecuteFunc.Invoke();
        }
        catch (BusinessLayerErrorException)
        {
        }
        catch (Exception e)
        {
            Debugger.Break();
            Logger.Error(e, "Execute action {actionName} failed", Name);
            if (ViewDomain.Default == null)
            {
#pragma warning disable CA2200
                throw;
#pragma warning restore CA2200
            }
            ViewDomain.Default.ShowErrorMessage(e.Message);
        }
    }

    public override string ToString()
    {
        return $"Action {Name}";
    }
}

// ReSharper disable once InconsistentNaming
public class UIAction<T> : BindableObjectBase, IAction<T>
{
    private string? _image;
    private bool _isVisible;
    internal Action<T?> ExecuteFunc;
    internal Func<T?, bool>? CanExecuteFunc;
    private bool? _imageIsBig;

    // ReSharper disable once StaticMemberInGenericType
    protected static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

    public UIAction(string name, Action<T?> execute, Func<T?, bool>? canExecute = null)
    {
        ArgumentNullException.ThrowIfNull(execute);
        Name = name;
        _isVisible = true;
        ExecuteFunc = execute;
        CanExecuteFunc = canExecute;
    }

    public string Name { get; }

    public string? Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }

    public bool? ImageIsBig
    {
        get => _imageIsBig;
        set => SetProperty(ref _imageIsBig, value);
    }

    public bool IsVisible
    {
        get => _isVisible;
        set => SetProperty(ref _isVisible, value);
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    public virtual bool CanExecute(T? arg)
    {
        try
        {
            return CanExecuteFunc?.Invoke(arg) ?? true;
        }
        catch (BusinessLayerErrorException)
        {
        }
        catch (Exception e)
        {
            Debugger.Break();
            Logger.Error(e, "CanExecute action {actionName} failed", Name);
        }

        return false;
    }

    public virtual void Execute(T? arg)
    {
        Logger.Debug("Call action {actionName}", Name);
        try
        {
            ExecuteFunc.Invoke(arg);
        }
        catch (BusinessLayerErrorException)
        {
        }
        catch (Exception e)
        {
            Debugger.Break();
            Logger.Error(e, "Execute action {actionName} failed", Name);
            if (ViewDomain.Default == null)
            {
                throw;
            }
            ViewDomain.Default.ShowErrorMessage(e.Message); // TODO: find a better way to report this here!
        }
    }

    public override string ToString()
    {
        return $"Action<{typeof(T).FullName}> {Name}";
    }
}