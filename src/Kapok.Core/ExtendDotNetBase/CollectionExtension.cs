// ReSharper disable once CheckNamespace
namespace System.Collections.Generic;

public static class CollectionExtension
{
    #region Range functions

    public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            collection.Add(item);
        }
    }

    public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> enumerable)
    {
        foreach (var item in enumerable)
        {
            collection.Remove(item);
        }
    }

    #endregion
}