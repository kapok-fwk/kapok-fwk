using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Kapok.BusinessLayer;
using Kapok.Data;
using Kapok.Entity;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using Res = Kapok.View.Resources.ListPage;

namespace Kapok.View;

/// <summary>
/// A base class for a list page.
/// </summary>
/// <typeparam name="TEntry"></typeparam>
public class ListPage<TEntry> : DataPage<TEntry>, IListPage<TEntry>
    where TEntry : class, new()
{
    private ObservableCollection<DataSetListView> _listViews = new();
    private DataSetListView? _currentListView;
    private IDataSetSelectionAction<TEntry>? _openCardPageAction;

    public ListPage(IViewDomain? viewDomain = null, IDataDomain? dataDomain = null)
        : base(viewDomain, dataDomain)
    {
        EditEntryAction = new UIDataSetSelectionAction<TEntry>("EditEntry", EditEntry, CanEditEntry) {Image = "tool-pencil", ImageIsBig = false };
        if (typeof(ISortableEntity).IsAssignableFrom(typeof(TEntry)))
        {
            SortUpEntryAction = new UIAction("SortUpEntry", SortUpEntry, CanSortUpEntry) {Image = "table-row-up"};
            SortDownEntryAction = new UIAction("SortDownEntry", SortDownEntry, CanSortDownEntry) {Image = "table-row-down"};
        }
        ExportAsExcelSheetAction = new UIAction("ExportAsExcelSheet", ExportAsExcelSheet) {Image = "export-to-excel"};
        ToggleFilterVisibleAction = new UIToggleAction("ToggleFilterVisible", ToggleFilterVisible, CanToggleFilterVisible) {Image = "filter", ImageIsBig = false, IsVisible = false};
        ClearUserFilterAction = new UIAction("ClearUserFilter", ClearUserFilter, CanClearUserFilter) {Image = "filter-cancel-2", ImageIsBig = false};

        ListViews.CollectionChanged += ListViews_CollectionChanged;

        Title = typeof(TEntry).GetDisplayAttributeNameOrDefault() ?? typeof(TEntry).FullName;
    }

    public ListPage(IViewDomain? viewDomain = null, IDataDomainScope? dataDomainScope = null)
        : base(viewDomain, dataDomainScope)
    {
        EditEntryAction = new UIDataSetSelectionAction<TEntry>("EditEntry", EditEntry, CanEditEntry) { Image = "tool-pencil", ImageIsBig = false };
        if (typeof(ISortableEntity).IsAssignableFrom(typeof(TEntry)))
        {
            SortUpEntryAction = new UIAction("SortUpEntry", SortUpEntry, CanSortUpEntry) { Image = "table-row-up" };
            SortDownEntryAction = new UIAction("SortDownEntry", SortDownEntry, CanSortDownEntry) { Image = "table-row-down" };
        }
        ExportAsExcelSheetAction = new UIAction("ExportAsExcelSheet", ExportAsExcelSheet) { Image = "export-to-excel" };
        ToggleFilterVisibleAction = new UIToggleAction("ToggleFilterVisible", ToggleFilterVisible, CanToggleFilterVisible) { Image = "filter", ImageIsBig = false, IsVisible = false };
        ClearUserFilterAction = new UIAction("ClearUserFilter", ClearUserFilter, CanClearUserFilter) { Image = "filter-cancel-2", ImageIsBig = false };

        ListViews.CollectionChanged += ListViews_CollectionChanged;

        Title = typeof(TEntry).GetDisplayAttributeNameOrDefault() ?? typeof(TEntry).FullName;
    }

    private void ListViews_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach(var newItem in e.NewItems.Cast<DataSetListView>())
            {
                newItem.SelectAction = new UIAction("SelectListView",
                    execute: () => CurrentListView = newItem,
                    canExecute: () => CurrentListView != newItem);
            }
        }
    }

    protected override IDataSetView<TEntry> InitializeBaseDataSet()
    {
        var dataSetView = ViewDomain.CreateDataSetView<TEntry>(DataDomainScope);
        UpdateBaseDataSetViewAllowedOption(dataSetView);
        return dataSetView;
    }

    private void UpdateBaseDataSetViewAllowedOption(IDataSetView<TEntry>? baseDataSetView)
    {
        if (baseDataSetView == null) return;
        baseDataSetView.InsertAllowed = AllowCreateNewEntry && (Editable || OpenCardPageAction != null);
        baseDataSetView.ModifyAllowed = Editable;
        baseDataSetView.DeleteAllowed = AllowDeleteEntry && (Editable || OpenCardPageAction != null);
    }

    protected override void OnLoaded()
    {
        if (!Editable)
            EditEntryAction.IsVisible = false;

        base.OnLoaded();

        // load list view metadata
        var listViews = MetadataEngine.ActiveMetadataEngine?.GetPageListViews(GetType(), typeof(TEntry));
        if (listViews != null)
        {
            // NOTE: use await here when OnLoaded is changed to support async.
            ListViews.AddRange(listViews.ToListAsync().Result);
        }

        if (DataSet == null || ((ICollection)DataSet.Columns).Count == 0 && CurrentListView == null && ListViews.Count > 0)
        {
            CurrentListView = ListViews[0];
        }

        UpdateBaseDataSetViewAllowedOption(DataSet);

#pragma warning disable CS8602
        DataSet.Load();
        DataSet.CanSaveChanged += DataSet_CanSaveChanged;
#pragma warning restore CS8602
    }

    protected override void OnClosed()
    {
        if (DataSet != null)
        {
            DataSet.CanSaveChanged -= DataSet_CanSaveChanged;
        }
        base.OnClosed();
    }

    private void DataSet_CanSaveChanged(object? sender, EventArgs e)
    {
        (SaveDataAction as UIAction)?.RaiseCanExecuteChanged();
    }

    /// <summary>
    /// The UI list views available for the user to select for the main DataSet.
    /// </summary>
    public ObservableCollection<DataSetListView> ListViews
    {
        get => _listViews;
#pragma warning disable CS8601
        set => SetProperty(ref _listViews, value);
#pragma warning restore CS8601
    }

    /// <summary>
    /// The current selected UI list view for the main DataSet.
    /// </summary>
    public DataSetListView? CurrentListView
    {
        get => _currentListView;
        set
        {
            var oldCurrentListView = _currentListView;
            if (SetProperty(ref _currentListView, value))
                OnCurrentListViewChanged(oldCurrentListView);
        }
    }

    private void OnCurrentListViewChanged(DataSetListView? oldDataSetListView)
    {
        if (DataSet == null) return;

        IDisposable? deferredRefresh = null;

        if (DataSet is DataSetView<TEntry> dataSet)
        {
            deferredRefresh = dataSet.DeferRefresh();
        }

        RequestSaveData(false);

        ((ICollection<PropertyView>)DataSet.Columns).Clear();

        // Note: In case the 'CurrentListView' is set to null we skip here the adding of columns and other changes..
        if (CurrentListView != null)
        {
            if (CurrentListView.Columns != null)
            {
                DataSet.Columns.AddRange(CurrentListView.Columns);
            }

            if (oldDataSetListView != null && oldDataSetListView.Filter != null && oldDataSetListView.Filter.FilterExpression != null)
                DataSet.Filter.Remove(new Filter<TEntry>((Expression<Func<TEntry, bool>>)oldDataSetListView.Filter.FilterExpression), layer: FilterLayer.Application);

            if (CurrentListView.Filter != null)
                DataSet.Filter.Add((IFilter<TEntry>)CurrentListView.Filter, FilterLayer.Application);

            DataSet.SortBy = CurrentListView.SortBy
                             ?? DataSet.GetDao().Model.PrimaryKeyProperties;
            DataSet.SortDirection = CurrentListView.SortDirection ?? SortDirection.Ascending;
        }

        // NOTE: We do not check here if the list uses calculated fields (which must be recalculated e.g. via 'Refresh()' to make sure that the numbers are correct.
        //       Since we don't check this here - as a workaround - we enforce always an refresh when the list view changes.
        if (DataSet.IsLoaded)
            DataSet.Refresh();

        deferredRefresh?.Dispose();
    }

    // ReSharper disable UnusedAutoPropertyAccessor.Global
    // ReSharper disable MemberCanBePrivate.Global
    [MenuItem, Display(Name = "OpenCardPageAction_Name", Description = "OpenCardPageAction_Description", GroupName = "Manage", Order = 2, ResourceType = typeof(Res))]
    public IDataSetSelectionAction<TEntry>? OpenCardPageAction
    {
        get => _openCardPageAction;
        protected set
        {
            if (SetProperty(ref _openCardPageAction, value))
            {
                UpdateBaseDataSetViewAllowedOption(DataSet);
            }
        }
    }

    [MenuItem, Display(Name = "EditEntryCommand_Name", Description = "EditEntryCommand_Description", GroupName = "Manage", Order = 3, ResourceType = typeof(Res))]
    public IDataSetSelectionAction<TEntry> EditEntryAction { get; }

    [MenuItem, Display(Name = "SortUpEntryCommand_Name", Description = "SortUpEntryCommand_Description", GroupName = "General", ResourceType = typeof(Res))]
    public IAction? SortUpEntryAction { get; }

    [MenuItem, Display(Name = "SortDownEntryCommand_Name", Description = "SortDownEntryCommand_Description", GroupName = "General", ResourceType = typeof(Res))]
    public IAction? SortDownEntryAction { get; }

    [MenuItem, Display(Name = "ExportAsExcelSheetCommand_Name", Description = "ExportAsExcelSheetCommand_Description", GroupName = "SendTo", Order = 1, ResourceType = typeof(Res))]
    public IAction ExportAsExcelSheetAction { get; }

    [MenuItem, Display(Name = "FilterVisibleToggleCommand_Name", Description = "FilterVisibleToggleCommand_Description", GroupName = "Page", Order = 100, ResourceType = typeof(Res))]
    public IToggleAction ToggleFilterVisibleAction { get; }

    [MenuItem, Display(Name = "ClearUserFilterCommand_Name", Description = "ClearUserFilterCommand_Description", GroupName = "Page", Order = 100, ResourceType = typeof(Res))]
    public IAction ClearUserFilterAction { get; }

    // ReSharper restore MemberCanBePrivate.Global
    // ReSharper restore UnusedAutoPropertyAccessor.Global

    protected override void CreateNewEntry()
    {
        if (EditMode == DataPageEditMode.View)
        {
            // in this case we create the new entry via the card page
        }

        base.CreateNewEntry();

        if (OpenCardPageAction != null)
        {
            var args = new[] {DataSet?.Current}.ToList();
            if (OpenCardPageAction.CanExecute(args))
            {
                OpenCardPageAction.Execute(args);
            }
        }
        else
        {
            ViewDomain.StartEditingDefaultDataGridCurrentEntity(this, enforceFirstEditableRow: true);
        }
    }

    protected virtual bool CanEditEntry(IList<TEntry?>? selectedEntries)
    {
        if (IsEditable)
            return true;

        if ((selectedEntries?.Count ?? 0) > 1)
            return false;

        return true;
    }

    protected virtual void EditEntry(IList<TEntry?>? selectedEntries)
    {
        ViewDomain.StartEditingDefaultDataGridCurrentEntity(this, false);
    }

    private static string StringFormatToExcelNumberFormat(Type type, string? stringFormat)
    {
        // excel build-in formats
        const string defaultFormat = "General"; // Build-In format 0
        const string defaultIntegerFormat = "0"; // Build-In format 1
        const string defaultFloatFormat = "#,##0.00"; // Build-In format 3
        const string defaultDateFormat = "mm-dd-yy"; // Build-In format 14
        const string defaultTimeFormat = "h:mm AM/PM"; // Built-In format 18
        const string defaultDateTimeFormat = "m/d/yy h:mm"; // Built-In format 22

        if (type.IsArray)
        {
#pragma warning disable CS8600
            type = type.GetElementType();
#pragma warning restore CS8600
            if (type == null)
                return defaultFormat;
        }

        if (type.IsSubclassOfRawGeneric(typeof(Nullable<>)))
        {
            type = type.GenericTypeArguments[0];
        }

        if (type == typeof(long) || type == typeof(ulong) ||
            type == typeof(int) || type == typeof(uint) ||
            type == typeof(short) || type == typeof(ushort) ||
            type == typeof(byte) || type == typeof(decimal) ||
            type == typeof(double) ||
            type == typeof(float))
        {
            // TODO: this is not a complete implementation of casting the 'StringFormat' logic to the excel format
            if (stringFormat != null)
            {
                switch (stringFormat)
                {
                    case "d":
                        return defaultIntegerFormat;
                    default:

                        if (stringFormat.StartsWith("p") || stringFormat.StartsWith("P"))
                        {
                            if (stringFormat.Length > 1)
                            {
                                if (int.TryParse(stringFormat.Substring(1), NumberStyles.Number,
                                        CultureInfo.InvariantCulture, out int decimalPlaces))
                                {
                                    if (decimalPlaces == 0)
                                        return "0%";
                                    if (decimalPlaces > 0)
                                    {
                                        string s = "0.";
                                        for (int i = 0; i < decimalPlaces; i++)
                                        {
                                            s += "0";
                                        }

                                        return s + "%";

                                        /*return new StringBuilder(3 + decimalPlaces)
                                            .Append("0.")
                                            .Insert(0, "0", decimalPlaces)
                                            .Append("%")
                                            .ToString();*/
                                    }
                                }

                                return "0.00%";
                            }
                        }
                        else
                        {
                            // if it only has the characters '#0,.'
                            char[] allowedChars = new[] {'#', '0', ',', '.'};

                            // checks for invalid types
                            if (stringFormat.All(cr => Array.Exists(allowedChars, c => c == cr)))
                            {
                                return stringFormat;
                            }
                        }

                        break;
                }
            }
            else if (type == typeof(long) || type == typeof(ulong) ||
                     type == typeof(int) || type == typeof(uint) ||
                     type == typeof(short) || type == typeof(ushort) ||
                     type == typeof(byte))
            {
                return defaultIntegerFormat;
            }
            else if (type == typeof(decimal) ||
                     type == typeof(double) ||
                     type == typeof(float))
            {
                return defaultFloatFormat;
            }
        }
        else if (type == typeof(DateTime))
        {
            switch (stringFormat)
            {
                case "d":
                    return defaultDateFormat;
                case "t":
                    return defaultTimeFormat;
                default:
#if DEBUG
                        if (!string.IsNullOrEmpty(stringFormat))
                            Debug.WriteLine(
                                $"ListPage: Excel export warning: Can't convert date time format '{stringFormat}' to excel format; use default build-in format 22 (m/d/yy h:mm).");
#endif
                    return defaultDateTimeFormat;
            }
        }

        return defaultFormat;
    }

    public static void ExportAsExcelSheet(IViewDomain viewDomain, IDataSetView<TEntry> dataSet, CultureInfo cultureInfo)
    {
        string? filename = viewDomain.OpenSaveFileDialog(
            title: "Save table content as excel sheet", // TODO: translation missing here
            fileMask: "Excel sheet (*.xlsx)|*.xlsx|All files|*"
        );

        if (filename == null)
            return;
            
        #region Read TableData to DataTable

        DataTable dataTable = new DataTable();

        var columnCache = new Dictionary<ColumnPropertyView, DataColumn>();

        foreach (var columnPropertyView in dataSet.Columns.Where(c => !((ColumnPropertyView)c).IsHidden).Cast<ColumnPropertyView>())
        {
            columnPropertyView.DeclaringType ??= typeof(TEntry);
            Debug.Assert(columnPropertyView.PropertyInfo != null);

            var exportType = columnPropertyView.PropertyInfo.PropertyType;
            if (exportType.IsSubclassOfRawGeneric(typeof(Nullable<>)))
            {
                exportType = exportType.GenericTypeArguments[0];
            }

            if (exportType.IsEnum)
            {
                exportType = typeof(string);
            }

            var originColumnName = columnPropertyView.Name;
            string columnName = originColumnName;

            // Check if column is already used - if yes, add an postfix number
            // we do this because the 'DataTable' object does not support having
            // two columns with the same name.
            int doubleColumnCounter = 2;
            while (dataTable.Columns.Contains(columnName))
            {
                columnName = $"{originColumnName} {doubleColumnCounter++}";
            }

            var dataColumn = new DataColumn(columnName, exportType);

            columnCache.Add(columnPropertyView, dataColumn);

            dataTable.Columns.Add(dataColumn);
        }

        foreach (var entry in dataSet.AsQueryable().ToList())
        {
            var newRow = dataTable.NewRow();

            foreach (var column in columnCache)
            {
#pragma warning disable CS8602
                var value = column.Key.PropertyInfo.GetMethod.Invoke(entry, null) ??
                            column.Key.PropertyInfo.PropertyType.GetTypeDefault();
#pragma warning restore CS8602

                if (column.Key.PropertyInfo.PropertyType.IsEnum)
                {
                    newRow[column.Value] = value != null ? EnumExtension.EnumValueToDisplayName(value, cultureInfo) : DBNull.Value;
                }
                else
                {
                    newRow[column.Value] = value ?? DBNull.Value;
                }
            }

            dataTable.Rows.Add(newRow);
        }
            
        #endregion

        #region write DataTable to excel file

        Stream? fileStream = null;
        try
        {
            fileStream = File.OpenWrite(filename);

            using var p = new ExcelPackage(fileStream);
            var worksheet = p.Workbook.Worksheets.Add("Sheet 1"); // TODO use resource for 'Sheet 1' and translate it

            worksheet.Cells.Style.Font.Size = 11;
            worksheet.Cells.Style.Font.Name = "Calibri";

            //Merging cells and create a center heading for out table

            if (dataTable.Columns.Count > 0)
            {
                // fill rows from dataTable
                worksheet.Cells[1, 1].LoadFromDataTable(dataTable, true, TableStyles.Light1);

                var columns = dataSet.Columns.Where(c => !((ColumnPropertyView) c).IsHidden).ToArray();

                // override the header with new captions and style
                int n = 1;
                foreach (DataColumn col in dataTable.Columns)
                {
                    var columnDefinition = columns[n - 1];

                    worksheet.Column(n).Style.Numberformat.Format = StringFormatToExcelNumberFormat(col.DataType, columnDefinition.StringFormat);

                    var cell = worksheet.Cells[1, n++];

                    cell.Value = columnDefinition.DisplayName?.LanguageOrDefault(CultureInfo.CurrentUICulture)
                                 ?? col.ColumnName;

                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    cell.Style.Border.Bottom.Style = cell.Style.Border.Top.Style = cell.Style.Border.Left.Style =
                        cell.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                // Auto size columns
                for (int j = 1; j <= dataTable.Columns.Count; j++)
                    worksheet.Cells[1, j, dataTable.Rows.Count + 1 /* add 1 for header line */, j].AutoFitColumns(10, 75);
            }

            p.Save();
        }
        finally
        {
            // ReSharper disable once ConstantConditionalAccessQualifier
            fileStream?.Close();
        }

        #endregion

        // open the file
        viewDomain.OpenFile(filename);
    }

    protected virtual void ExportAsExcelSheet()
    {
        if (DataSet == null) return;
        ExportAsExcelSheet(ViewDomain, DataSet, ViewDomain.Culture);
    }

    protected virtual bool CanSortUpEntry()
    {
        return DataSet?.CanSortUp() ?? false;
    }

    protected virtual bool CanSortDownEntry()
    {
        return DataSet?.CanSortDown() ?? false;
    }

    protected virtual void SortUpEntry()
    {
        EndEdit();
        if (RequestSaveData())
            DataSet?.SortUp();
    }

    protected virtual void SortDownEntry()
    {
        EndEdit();
        if (RequestSaveData())
            DataSet?.SortDown();
    }

    protected virtual bool CanToggleFilterVisible()
    {
        return DataSet?.ToggleFilterVisibleAction.CanExecute() ?? false;
    }

    protected virtual void ToggleFilterVisible()
    {
        if (DataSet == null) return;

        DataSet?.ToggleFilterVisibleAction.Execute();

        // We set here the `IsChecked` field manually, because the command is bound
        // with an short-key, and when using the short-key instead of an ToggleButton
        // control, the IsChecked field is not automatically set.
        ToggleFilterVisibleAction.IsChecked = DataSet?.IsFilterVisible ?? false;
    }

    protected virtual bool CanClearUserFilter()
    {
        return DataSet?.ClearUserFilterAction.CanExecute() ?? false;
    }

    protected virtual void ClearUserFilter()
    {
        if (RequestSaveData())
            DataSet?.ClearUserFilterAction.Execute();
    }

    protected override void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        // pass config of Insert/Modify/Delete allowed over to the base DataSetView
        if (// does not run when BaseDataSet is not yet initialized
            DataSets.ContainsKey(BaseDataSet) &&
            (propertyName == nameof(AllowCreateNewEntry) ||
             propertyName == nameof(Editable) ||
             propertyName == nameof(AllowDeleteEntry)))
        {
            UpdateBaseDataSetViewAllowedOption(DataSet);
        }

        base.OnPropertyChanged(propertyName);
    }

    #region IListPage

    IDataSetListView? IListPage.CurrentListView
    {
        get => CurrentListView;
        set => CurrentListView = (DataSetListView?)value;
    }

    IEnumerable<IDataSetListView> IListPage.ListViews => ListViews;

    IAction IListPage.EditEntryAction => new UIAction(EditEntryAction.Name, IListPage_EditEntry, IListPage_CanEditEntry);

    private bool IListPage_CanEditEntry()
    {
        if (DataSet == null || DataSet.Current == null)
            return false;

        return EditEntryAction.CanExecute(new List<TEntry?> { DataSet.Current });
    }
    
    private void IListPage_EditEntry()
    {
        if (DataSet == null || DataSet.Current == null)
            return;

        EditEntryAction.Execute(new List<TEntry?> { DataSet.Current });
    }
    
    #endregion
}