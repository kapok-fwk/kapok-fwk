using System.ComponentModel;
using System.Drawing;

namespace Kapok.View;

public class AddingNewEntryEventArgs : EventArgs
{
    public AddingNewEntryEventArgs(object newEntry)
    {
        NewEntry = newEntry;
    }

    public object NewEntry { get; set; }
}

public class NewEntryAddedEventArgs : EventArgs
{
    public NewEntryAddedEventArgs(object newEntry)
    {
        NewEntry = newEntry;
    }

    public object NewEntry { get; set; }
}

public class DeletingEntryEventArgs : EventArgs
{
    public DeletingEntryEventArgs(object deleteEntry)
    {
        DeleteEntry = deleteEntry;
    }

    public object DeleteEntry { get; set; }
}

public class EntryPropertyChangingEventArgs : PropertyChangingEventArgs
{
    public EntryPropertyChangingEventArgs(object entry, string propertyName) : base(propertyName)
    {
        Entry = entry;
    }

    public object Entry { get; set; }
}

public class EntryPropertyChangedEventArgs : PropertyChangedEventArgs
{
    public EntryPropertyChangedEventArgs(object entry, string propertyName) : base(propertyName)
    {
        Entry = entry;
    }

    public object Entry { get; set; }
}

public class EntryDataErrorsChangedEventArgs : DataErrorsChangedEventArgs
{
    public EntryDataErrorsChangedEventArgs(object entry, string propertyName) : base(propertyName)
    {
        Entry = entry;
    }

    public object Entry { get; set; }
}
    
public class DataSetEntityColoringEventArgs : EventArgs
{
    public DataSetEntityColoringEventArgs(object entity, string propertyName)
    {
        Entity = entity;
        PropertyName = propertyName;
    }

    public object Entity { get; }

    public string? PropertyName { get; }

    /// <summary>
    /// The text foreground color
    /// </summary>
    public Color? ForegroundColor { get; set; }

    /// <summary>
    /// The text foreground color when the entity is selected
    /// e.g. in a UI list
    /// </summary>
    public Color? ForegroundSelectedColor { get; set; }

    /// <summary>
    /// The cell background color
    /// </summary>
    public Color? BackgroundColor { get; set; }

    /// <summary>
    /// The cell background color when the entity is selected
    /// e.g. in a UI list
    /// </summary>
    public Color? BackgroundSelectedColor { get; set; }
}

public interface IDataSetView : IDataSetReadonlyView, IDisposable
{
    //IReadOnlyDictionary<string, PropertyLookupView<TEntry, TRepository>> PropertyLookup { get; }

    bool IsDeferredSaveActive { get; }

    bool IsNewEntry { get; }

    bool InsertAllowed { get; set; }
    bool ModifyAllowed { get; set; }
    bool DeleteAllowed { get; set; }

    void Add(object entry);
    void AddRange(IEnumerable<object> entries);
    void Remove(object entry);
    void RemoveRange(IEnumerable<object> entries);
    void InitNewEntry(object entry);

    void PrepareSave();

    IQueryable AsQueryableForUpdate();

    /// <summary>
    /// Returns true when the DataSet has entries which have validation errors.
    /// </summary>
    bool HasValidationErrors { get; }

    /// <summary>
    /// Is called when 'CanSave' changed and IsDeferredSaveActive is <c>true</c>.
    /// </summary>
    event EventHandler CanSaveChanged;

    /// <summary>
    /// Gives back if an save of data is possible.
    ///
    /// This will return false when changes where made, but
    /// there exist some validation errors.
    /// </summary>
    /// <returns></returns>
    bool CanSave();

    /// <summary>
    /// Saves the changes which where made and are not yet saved.
    /// </summary>
    void Save();

    void RejectChanges();

    IAction CreateNewEntryAction { get; }

    IDataSetSelectionAction DeleteEntryAction { get; }

    // moving NOTE maybe this should be moved into a dedicated interface because it does not work with all entries
    /*ICommand MoveInCommand { get; }
    ICommand MoveOutCommand { get; }*/

    /// <summary>
    /// Event is executed when a new entry is added to the table.
    /// 
    /// The business layer method InitNewEntry() has been called, but not:
    /// * the New() business layer function
    /// * the object is not jet added to the collection.
    ///
    /// This is the perfect moment to add some initial data before the user starts with editing the entry.
    /// </summary>
    event EventHandler<AddingNewEntryEventArgs> AddingNewEntry;

    /// <summary>
    /// Event is executed when new entry has been added to the table.
    /// </summary>
    event EventHandler<NewEntryAddedEventArgs> NewEntryAdded;

    /// <summary>
    /// Event is executed when an new entry is deleted, before it is deleted from the repository.
    /// </summary>
    event EventHandler<DeletingEntryEventArgs> DeletingEntry;

    /// <summary>
    /// Event is executed when an 'PropertyChanging' event is invoked from an entry of the list.
    /// </summary>
    event EventHandler<EntryPropertyChangingEventArgs> EntryPropertyChanging;

    /// <summary>
    /// Event is executed when an 'PropertyChanged' event is invoked from an entry of the list.
    /// </summary>
    event EventHandler<EntryPropertyChangedEventArgs> EntryPropertyChanged;

    /// <summary>
    /// Event is executed when an 'ErrorsChanged' event is invoked from an entry of the list.
    /// </summary>
    event EventHandler<EntryDataErrorsChangedEventArgs> EntryErrorsChanged;

    event EventHandler<DataSetEntityColoringEventArgs> EntryColoring;
}

public interface IDataSetView<TEntry> : IDataSetReadonlyView<TEntry>, IDataSetView
    where TEntry : class, new()
{
    new IDataSetSelectionAction<TEntry> DeleteEntryAction { get; }

    void Add(TEntry entry);
    void AddRange(IEnumerable<TEntry> entries);
    void Remove(TEntry entry);
    void RemoveRange(IEnumerable<TEntry> entries);
    void InitNewEntry(TEntry entry);

    new IQueryable<TEntry> AsQueryableForUpdate();
}