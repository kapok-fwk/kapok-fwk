namespace System.Collections.ObjectModel;

public static class ObservableCollectionExtension
{
    public static void Sort<T>(this ObservableCollection<T> collection, Comparison<T> comparison)
    {
        var sortableList = new List<T>(collection);
        sortableList.Sort(comparison);

        for (int i = 0; i < sortableList.Count; i++)
        {
            collection.Move(collection.IndexOf(sortableList[i]), i);
        }
    }

    #region Add support for range (not notification-optimized)

    // AddRange<T> is covered by ICollection<T>.AddRange<T> extension
    // RemoveRange<T> is covered by ICollection<T>.RemoveRange<T> extension

    public static void InsertRange<T>(this ObservableCollection<T> observableCollection, int index, IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            observableCollection.Insert(index++, item);
        }
    }

    #endregion
}