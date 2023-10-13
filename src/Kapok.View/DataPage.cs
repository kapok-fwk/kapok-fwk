using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Kapok.Data;
using Kapok.Entity;
using Res = Kapok.View.Resources.DataPage;

namespace Kapok.View;

/// <summary>
/// A base class for a page connected to one or multiple data sets.
/// </summary>
/// <typeparam name="TEntry"></typeparam>
public abstract class DataPage<TEntry> : InteractivePage, IDataPage<TEntry>
    where TEntry : class, new()
{
    // ReSharper disable once MemberCanBeProtected.Global
    // ReSharper disable once ConvertToConstant.Global
    // ReSharper disable once StaticMemberInGenericType
    public static string BaseDataSet = "Base";

    private bool _editable = true;
    private bool _allowCreateNewEntry = true;
    private bool _allowDeleteEntry = true;
    private bool _isLoaded;
    private readonly bool _tableDataPassedOnConstruction;
    private IDataDomainScope? _dataDomainScope;

    private readonly Dictionary<string, IDataSetView> _dataSets = new();

    protected DataPage(IViewDomain? viewDomain = null, IDataDomain? dataDomain = null)
        : base(viewDomain)
    {
        DataDomain = dataDomain
                     ?? Data.DataDomain.Default
                     ?? throw new NotSupportedException(
                         $"You have to first set Kapok.Core.DataDomain.Default before you can initiate a page without {nameof(dataDomain)} being provided");

        // init actions
        RefreshAction = new UIAction("RefreshPage", RefreshActionCall) { Image = "symbol-refresh" };
        SaveDataAction = new UIAction("SaveDataOnPage", () => SaveData(), CanSaveData) { Image = "save", ImageIsBig = false };
        CreateNewEntryAction = new UIAction("CreateNewEntry", CreateNewEntry, CanCreateNewEntry) { Image = "document-new", ImageIsBig = true };
        DeleteEntryAction = new UIDataSetSelectionAction<TEntry>("DeleteEntry", DeleteEntry, CanDeleteEntry) { Image = "symbol-delete", ImageIsBig = false };
        ToggleEditModeAction = new UIToggleAction("ToggleEditMode", ToggleEditMode, CanToggleEditMode) { Image = "tool-pencil", ImageIsBig = false, IsVisible = false };
    }

    protected DataPage(IViewDomain? viewDomain = null, IDataDomainScope? dataDomainScope = null)
        : this(viewDomain, dataDomainScope?.DataDomain)
    {
        _dataDomainScope = dataDomainScope;
    }

    protected DataPage(IDataSetView<TEntry> dataSet, IViewDomain? viewDomain = null, IDataDomain? dataDomain = null)
        : this(viewDomain, dataDomain)
    {
        _tableDataPassedOnConstruction = true;

        AddDataSet(BaseDataSet, dataSet);
    }

    protected DataPage(IDataSetView<TEntry> dataSet, IViewDomain? viewDomain = null, IDataDomainScope? dataDomainScope = null)
        : this(dataSet, viewDomain, dataDomainScope?.DataDomain)
    {
        _dataDomainScope = dataDomainScope;
    }

    protected IDataDomain DataDomain { get; }

    protected IDataDomainScope DataDomainScope => _dataDomainScope ??= DataDomain.CreateScope();

    public IDataSetView<TEntry>? DataSet
    {
        get
        {
            if (!_dataSets.ContainsKey(BaseDataSet))
            {
                if (_isLoaded)
                    return null; // probably was disposed after OnClose() was called

                // not loaded yet, initialize the base data set with an default type
                var baseDataSet = InitializeBaseDataSet();
                AddDataSet(BaseDataSet, baseDataSet);
                Menu[UIMenu.BaseMenuName].DefaultReferencingDataSet = baseDataSet;
            }

            return (IDataSetView<TEntry>) _dataSets[BaseDataSet];
        }
    }

    protected virtual IDataSetView<TEntry> InitializeBaseDataSet()
    {
        throw new NotSupportedException("The base data set was not defined.");
    }

    public IReadOnlyDictionary<string, IDataSetView> DataSets => _dataSets;

    public bool HasValidationErrors => DataSets.Any(ds => ds.Value.HasValidationErrors);

    /// <summary>
    /// If the DataSet is editable.
    /// 
    /// Note: This should not be changed after initializing of the Page!
    /// </summary>
    [Display(Name = "Editable", Description = "Editable_Description", ResourceType = typeof(Res))]
    public bool Editable
    {
        get => _editable;
        set
        {
            if (_editable == value) return;
            _editable = value;

            bool editModeChanged = false;
            if (!_editable && _editMode == DataPageEditMode.Edit)
            {
                _editMode = DataPageEditMode.View;
                editModeChanged = true;
            }

            OnPropertyChanged();
            OnPropertyChanged(nameof(IsEditable));
            if (editModeChanged)
                OnPropertyChanged(nameof(EditMode));
        }
    }

    [Display(Name = "IsEditable", Description = "IsEditable_Description", ResourceType = typeof(Res))]
    public bool IsEditable => Editable && DataSets.ContainsKey(BaseDataSet) && DataSets[BaseDataSet].ModifyAllowed && EditMode == DataPageEditMode.Edit;

    private DataPageEditMode _editMode = DataPageEditMode.Edit; // when possible always start in edit mode
    public DataPageEditMode EditMode
    {
        get => _editMode;
        set
        {
            if (_editMode == value) return;
            _editMode = value;
            OnPropertyChanged();
            if (Editable && DataSets.ContainsKey(BaseDataSet) && DataSets[BaseDataSet].ModifyAllowed)
                OnPropertyChanged(nameof(IsEditable));
        }
    }

    // TODO: when this is set to false, the menu item should not be visible
    // TODO: I don't like the naming 'Allow' because it does not set any permissions, its just a visual appearance in a page/form...
    public bool AllowCreateNewEntry
    {
        get => _allowCreateNewEntry;
        protected set => SetProperty(ref _allowCreateNewEntry, value);
    }

    // TODO: when this is set to false, the menu item should not be visible
    public bool AllowDeleteEntry
    {
        get => _allowDeleteEntry;
        protected set => SetProperty(ref _allowDeleteEntry, value);
    }

    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable MemberCanBePrivate.Global
    [MenuItem, Display(Name = "SaveDataCommand_Name", Description = "SaveDataCommand_Description", GroupName = "Page", Order = 1, ResourceType = typeof(Res))]
    public IAction SaveDataAction { get; }

    [MenuItem, Display(Name = "RefreshCommand_Name", Description = "RefreshCommand_Description", GroupName = "Page", Order = 0, ResourceType = typeof(Res))]
    public IAction RefreshAction { get; }

    [MenuItem, Display(Name = "CreateNewEntryCommand_Name", GroupName = "Manage", Order = 1, ResourceType = typeof(Res))]
    public IAction CreateNewEntryAction { get; }

    [MenuItem, Display(Name = "DeleteEntryCommand_Name", GroupName = "Manage", Order = 4, ResourceType = typeof(Res))]
    public IDataSetSelectionAction<TEntry> DeleteEntryAction { get; }

    [MenuItem, Display(Name = "EditModeToggleCommand_Name", GroupName = "Page", Order = 10, ResourceType = typeof(Res))]
    public IToggleAction ToggleEditModeAction { get; }
    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Global

    protected void AddDataSet(string name, IDataSetView dataSet)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException($"Parameter {nameof(name)} cannot be null or empty.", nameof(name));

        if (_dataSets.ContainsKey(name))
            throw new ArgumentException($"The data set with the name '{name}' exist already in the page.", nameof(name));

        if (name == BaseDataSet)
        {
            if (!(dataSet is IDataSetView<TEntry>))
                throw new ArgumentException($"The base data set must be assignable with the interface {typeof(IDataSetView<TEntry>).FullName}.", nameof(dataSet));

            dataSet.PropertyChanged += BaseDataSet_PropertyChanged;
        }

        _dataSets.Add(name, dataSet);
    }

    protected IDataSetView<TDataSetEntity> GetDataSet<TDataSetEntity>(string? name = null)
        where TDataSetEntity : class, new()
    {
        if (name == null)
            name = typeof(TDataSetEntity).Name;

        if (!_dataSets.ContainsKey(name))
            throw new ArgumentException($"The data set with the name '{name}' does not exist.", nameof(name));

        return (IDataSetView<TDataSetEntity>)DataSets[name];
    }

    protected void DisposeDataSet(string name)
    {
        if (!_dataSets.ContainsKey(name))
            throw new ArgumentException($"The data set with the name '{name}' does not exist, it is maybe already disposed. You can't dispose an data set twice.", nameof(name));

        if (name == BaseDataSet)
            _dataSets[name].PropertyChanged -= BaseDataSet_PropertyChanged;

        _dataSets[name].Dispose();
        _dataSets.Remove(name);
    }

    private void BaseDataSet_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IDataSetView<TEntry>.ModifyAllowed))
            OnPropertyChanged(nameof(IsEditable));
    }

    #region Local event handling

    protected override void OnLoaded()
    {
        // make sure that the data set is initialized, so that the 'ReferencingDataSet' in the base menu is set
        // ReSharper disable once UnusedVariable
#pragma warning disable IDE0059
        var x = DataSet;
#pragma warning restore IDE0059

        // initiate EditMode toggle command
        if (IsEditable)
            ToggleEditModeAction.IsChecked = true;

        // hide EditMode toggle command when editing is disabled for the page
        if (!Editable)
            ToggleEditModeAction.IsVisible = false;

        if (!AllowCreateNewEntry)
            CreateNewEntryAction.IsVisible = false;

        if (!AllowCreateNewEntry)
            DeleteEntryAction.IsVisible = false;

        if (!IsEditable && !AllowCreateNewEntry && !AllowDeleteEntry)
            SaveDataAction.IsVisible = false;

        DataDomainScope.RegisterUsage(this);

        base.OnLoaded();

        _isLoaded = true;
    }

    protected override void OnClosing(CancelEventArgs args)
    {
        base.OnClosing(args);

        EndEdit();

        if (DataSets.Any(ds => ds.Value.HasValidationErrors))
        {
            if (AskRevertChangesDueToValidationErrors())
            {
                foreach (var dataSet in DataSets.Values)
                {
                    if (dataSet.HasValidationErrors)
                    {
                        dataSet.RejectChanges();
                    }
                }
            }
            else
            {
                args.Cancel = true;
                return;
            }
        }

        if (SaveDataAction.CanExecute())
        {
            if (!RequestSaveData())
            {
                args.Cancel = true;
            }
        }
    }

    private bool AskRevertChangesDueToValidationErrors()
    {
        var revertChangesButton = new QuestionDialogPage.DialogButton
        {
            Image = "button-error",
            Label = Res.AskRevertChangesDueToValidationErrorsDialog_RevertChanges_Label,
            Description = Res.AskRevertChangesDueToValidationErrorsDialog_RevertChanges_Description
        };
        var goBackButton = new QuestionDialogPage.DialogButton
        {
            Image = "command-undo",
            Label = Res.AskRevertChangesDueToValidationErrorsDialog_GoBack_Label,
            Description = Res.AskRevertChangesDueToValidationErrorsDialog_GoBack_Description,
            IsDefault = true
        };

        // TODO: would be nice to show at least one of the validation errors in this dialog, with entry primary key
        var dialog = new QuestionDialogPage(ViewDomain);
        dialog.Title = Res.AskRevertChangesDueToValidationErrorsDialog_Title;
        dialog.Message = Res.AskRevertChangesDueToValidationErrorsDialog_Message;
        dialog.DialogButtons.AddRange(new []
        {
            revertChangesButton,
            goBackButton
        });
        dialog.ShowDialog(this);

        return dialog.DialogResultButton == revertChangesButton;
    }

    protected override void OnClosed()
    {
        base.OnClosed();

        if (!_tableDataPassedOnConstruction)
        {
            DisposeDataSet(BaseDataSet);
        }

        DataDomainScope.UnregisterUsage(this);
    }

    #endregion

    /// <summary>
    /// Is executed right before SaveData() is executed.
    ///
    /// This is the right place to make sure that every new record is added to a collection etc.
    /// </summary>
    protected virtual void PrepareSaveData()
    {
        EndEdit();
        foreach (var dataSet in _dataSets.Values)
        {
            dataSet.PrepareSave();
        }
    }

    protected virtual void RejectChanges()
    {
        // TODO: I don't know if we should call here 'EndEdit()'. This should be checked.

        foreach (var dataSet in _dataSets.Values)
        {
            dataSet.RejectChanges();
        }
    }

    /// <summary>
    /// Sends a user request if changes shall be saved.
    /// </summary>
    /// <returns>
    /// Returns <code>true</code> when no changes existed, the changes where saved or the changes where rejected.
    /// Returns <code>false</code> when the user canceled the action.
    /// </returns>
    public bool RequestSaveData(bool allowCancel = true)
    {
        if (SaveDataAction.CanExecute())
        {
            // TODO: Would be cool to have a checkbox in the dialog which automatically answers the question when the user has checked "don't ask anymore"
            var result = UnsavedChangesDialogPage.ShowDialog(this, allowCancel);
            switch (result)
            {
                case UnsavedChangesDialogPage.UnsavedChangesDialogResult.Save:
                    SaveData();
                    break;
                case UnsavedChangesDialogPage.UnsavedChangesDialogResult.Discard:
                    RejectChanges();
                    break;
                case UnsavedChangesDialogPage.UnsavedChangesDialogResult.Cancel:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (HasValidationErrors)
        {
            // TODO: would be nice to show at least one of the validation errors in this message, with entry primary key
            ViewDomain.ShowErrorMessage(Res.ValidationErrorExist_Message_Text, Res.ValidationErrorExist_Message_Title, this);
            return false;
        }

        return true;
    }

    protected virtual void Refresh()
    {
        foreach (var dataSet in _dataSets.Values)
        {
            dataSet.Refresh();
        }
    }

    /// <summary>
    /// Forces all binding objects to write it's data to the source.
    /// </summary>
    // TODO: this method should not be virtual, but it has to because of an hack implemented in DetailPage<T>
    protected virtual void EndEdit()
    {
        ViewDomain.PageEndEdit(this);
    }

    #region Action implementation methods

    protected virtual bool CanSaveData()
    {
        return !HasValidationErrors &&
               DataSets.Any(ds => ds.Value.CanSave());
    }

    /// <summary>
    /// Save the data.
    /// </summary>
    /// <returns>
    /// Returns true when the save action was successful.
    /// </returns>
    protected virtual bool SaveData()
    {
        StringBuilder FormatException(Exception ex)
        {
            var sb = new StringBuilder();
            sb.Append(ex.Message);

            if (ex.InnerException != null)
            {
                sb.AppendLine();
                sb.AppendLine("--");
                sb.Append(ex.InnerException.Message);
            }

            return sb;
        }

        try
        {
            PrepareSaveData();
            foreach (var dataSet in _dataSets.Values)
            {
                dataSet.Save();
            }

            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            ViewDomain.ShowErrorMessage(
                Res.DbUpdateConcurrencyException_MessageBox_Text,
                Res.DbUpdateConcurrencyException_MessageBox_Caption,
                this);
        }
        catch (DbUpdateException ex)
        {
            ViewDomain.ShowErrorMessage(
                string.Format(Res.DbUpdateException_MessageBox_Text, FormatException(ex)),
                Res.DbUpdateException_MessageBox_Caption,
                this);
        }
        catch (DuplicatedEntryException ex)
        {
            ViewDomain.ShowErrorMessage(
                string.Format(Res.DuplicatedEntryException_MessageBox_Text, FormatException(ex)),
                Res.DuplicatedEntryException_MessageBox_Caption,
                this);
        }

        return false;
    }

    private void RefreshActionCall()
    {
        if (RequestSaveData())
        {
            Refresh();
        }
    }

    protected virtual bool CanCreateNewEntry()
    {
        return AllowCreateNewEntry && DataSets.ContainsKey(BaseDataSet) && DataSets[BaseDataSet].CreateNewEntryAction.CanExecute();
    }

    protected virtual void CreateNewEntry()
    {
        DataSet?.CreateNewEntryAction.Execute();
    }

    protected virtual bool CanDeleteEntry(IList<TEntry?>? selectedEntries)
    {
        return AllowDeleteEntry && (DataSet?.DeleteEntryAction.CanExecute(selectedEntries) ?? false);
    }

    protected virtual void DeleteEntry(IList<TEntry?>? selectedEntries)
    {
        DataSet?.DeleteEntryAction.Execute(selectedEntries);
    }

    protected virtual bool CanToggleEditMode()
    {
        return EditMode == DataPageEditMode.Edit || Editable;
    }
    protected virtual void ToggleEditMode()
    {
        if (!ToggleEditModeAction.IsChecked)
        {
            EditMode = DataPageEditMode.View;
        }
        else
        {
            EditMode = DataPageEditMode.Edit;
        }
    }

    #endregion


    #region IDataPage

#pragma warning disable CS8768
    IDataSetView? IDataPage.DataSet => DataSet;
#pragma warning restore CS8768

    IAction IDataPage.DeleteEntryAction => new UIAction(DeleteEntryAction.Name, IDataPage_DeleteEntry, IDataPage_CanDeleteEntry);

    private bool IDataPage_CanDeleteEntry()
    {
        return DeleteEntryAction.CanExecute(DataSet?.SelectedEntries);
    }

    private void IDataPage_DeleteEntry()
    {
        DeleteEntryAction.Execute(DataSet?.SelectedEntries);
    }
    
    #endregion

    #region IDataPage<TEntry>

#pragma warning disable CS8768
    IDataSetView<TEntry>? IDataPage<TEntry>.DataSet => DataSet;
#pragma warning restore CS8768

    #endregion
}