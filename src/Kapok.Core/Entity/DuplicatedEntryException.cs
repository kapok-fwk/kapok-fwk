namespace Kapok.Entity;

public class DuplicatedEntryException : Exception
{
    public DuplicatedEntryException(object existingEntry, object newEntry)
        : base(
            $"The entry {newEntry} already exist in the table ({existingEntry}). Probably you tried to use a primary key twice.")
    {
        ExistingEntry = existingEntry;
        NewEntry = newEntry;
    }

    public object ExistingEntry { get; }

    public object NewEntry { get; }
}