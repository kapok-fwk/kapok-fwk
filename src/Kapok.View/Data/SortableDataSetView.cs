using System.Collections.Specialized;
using System.Diagnostics;
using Kapok.Entity;
using Res = Kapok.View.Resources.Data.SortableTableDataViewModel;

namespace Kapok.View;
// TODO: [minor] [performance] today the sort number always is added by 1. It might be a good idea to set this up to e.g. 100 so when new items are inserted between items, we avoid to scroll through the whole following list and adding a +1 to the sort order (but then +100 should only be used for new items at the end of a list, not for each adding!)

public static class SortableDataSetView
{
    /// <summary>
    /// Checks if the current entry can be sorted up.
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    /// <param name="tableData"></param>
    /// <returns></returns>
    public static bool CanSortUp<TEntry>(this IDataSetView<TEntry> tableData)
        where TEntry : class, new()
    {
        if (!typeof(ISortableEntity).IsAssignableFrom(typeof(TEntry)))
            throw new NotSupportedException($"The method {nameof(CanSortUp)} can only be used with an TEntry which implements the interface {typeof(ISortableEntity).FullName}.");

        return tableData.Current != null &&
               (from m in tableData.Collection.Cast<ISortableEntity>()
                   where m.SortOrder < ((ISortableEntity)tableData.Current).SortOrder
                   orderby m.SortOrder descending
                   select m
               ).FirstOrDefault() != null;
    }

    /// <summary>
    /// checks if the current entry can beo sorted down.
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    /// <param name="tableData"></param>
    /// <returns></returns>
    public static bool CanSortDown<TEntry>(this IDataSetView<TEntry> tableData)
        where TEntry : class, new()
    {
        if (!typeof(ISortableEntity).IsAssignableFrom(typeof(TEntry)))
            throw new NotSupportedException($"The method {nameof(CanSortDown)} can only be used with an TEntry which implements the interface {typeof(ISortableEntity).FullName}.");
            
        return tableData.Current != null &&
               (from m in tableData.Collection.Cast<ISortableEntity>()
                   where m.SortOrder > ((ISortableEntity)tableData.Current).SortOrder
                   orderby m.SortOrder ascending
                   select m
               ).FirstOrDefault() != null;
    }

    /// <summary>
    /// Move the current entry down in the sorting order.
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    /// <param name="tableData"></param>
    public static void SortUp<TEntry>(this IDataSetView<TEntry> tableData)
        where TEntry : class, new()
    {
        if (!typeof(ISortableEntity).IsAssignableFrom(typeof(TEntry)))
            throw new NotSupportedException($"The method {nameof(SortUp)} can only be used with an TEntry which implements the interface {typeof(ISortableEntity).FullName}.");

        var currMember = tableData.Current as ISortableEntity;
        if (currMember == null)
        {
            Debug.WriteLineIf(currMember != null, "IDataSetView<>.SortUp not possible, because entity is not assignable to ISortableEntity");
            return;
        }

        var prevMember = (from m in tableData.Collection.Cast<ISortableEntity>()
                where m.SortOrder < currMember.SortOrder
                orderby m.SortOrder descending
                select m
            ).FirstOrDefault();
        if (prevMember == null)
            throw new NotSupportedException(Res.SortUp_IsAlreadyFirstElement);

        prevMember.SortOrder++;
        currMember.SortOrder--;
        tableData.Save();
        tableData.Refresh();
    }

    /// <summary>
    /// Move the current entry up in the sorting order.
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    /// <param name="tableData"></param>
    public static void SortDown<TEntry>(this IDataSetView<TEntry> tableData)
        where TEntry : class, new()
    {
        if (!typeof(ISortableEntity).IsAssignableFrom(typeof(TEntry)))
            throw new NotSupportedException($"The method {nameof(SortDown)} can only be used with an TEntry which implements the interface {typeof(ISortableEntity).FullName}.");

        var currMember = tableData.Current as ISortableEntity;
        if (currMember == null)
        {
            Debug.WriteLineIf(currMember != null, "IDataSetView<>.SortUp not possible, because entity is not assignable to ISortableEntity");
            return;
        }

        var nextMember = (from m in tableData.Collection.Cast<ISortableEntity>()
                where m.SortOrder > currMember.SortOrder
                orderby m.SortOrder ascending
                select m
            ).FirstOrDefault();

        if (nextMember == null)
            throw new NotSupportedException(Res.SortDown_IsAlreadyLastElement);

            

        nextMember.SortOrder--;
        currMember.SortOrder++;
        tableData.Save();
        tableData.Refresh();
    }

    /// <summary>
    /// Necessary when the entries in the TableData will be editable.
    /// </summary>
    /// <typeparam name="TEntry"></typeparam>
    /// <param name="dataSet"></param>
    public static void AddBusinessLogic<TEntry>(IDataSetView<TEntry> dataSet)
        where TEntry : class, new()
    {
        if (!typeof(ISortableEntity).IsAssignableFrom(typeof(TEntry)))
            throw new NotSupportedException($"The {nameof(TEntry)} type parameter needs to implement {typeof(ISortableEntity).FullName} to be able to use the method {nameof(SortableDataSetView)}.{nameof(AddBusinessLogic)}.");

        ((INotifyCollectionChanged)dataSet.Collection).CollectionChanged += CollectionChanged<TEntry>;
    }

    public static void RemoveBusinessLogic<TEntry>(IDataSetView<TEntry> dataSet)
        where TEntry : class, new()
    {
        if (!typeof(ISortableEntity).IsAssignableFrom(typeof(TEntry)))
            throw new NotSupportedException($"The {nameof(TEntry)} type parameter needs to implement {typeof(ISortableEntity).FullName} to be able to use the method {nameof(SortableDataSetView)}.{nameof(AddBusinessLogic)}.");

        ((INotifyCollectionChanged)dataSet.Collection).CollectionChanged -= CollectionChanged<TEntry>;
    }

    private static void CollectionChanged<TEntry>(object? sender, NotifyCollectionChangedEventArgs e)
        where TEntry : class
    {
        var collection = (ICollection<TEntry>?) sender;
        if (collection == null)
            throw new ArgumentException($"The parameter {nameof(sender)} cannot be cast into {nameof(ICollection<TEntry>)}.");

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
            {
                if (e.NewItems == null) return;
                Debug.Assert(collection.Count >= e.NewItems.Count, "collection.Count >= e.NewItems.Count is not true!");

                int startSortOrder;
                bool searchForFollowingItems;
                if (e.NewStartingIndex <= 0)
                {
                    startSortOrder = 1;
                    searchForFollowingItems = collection.Count > e.NewItems.Count;
                }
                else
                {
                    startSortOrder = collection.Cast<ISortableEntity>().ToList()[e.NewStartingIndex - 1].SortOrder + 1; // TODO: [minor] [performance] we do here twice a enumeration of the collection ...
                    searchForFollowingItems = true;
                }

                var nextSortOrder = startSortOrder;
                foreach (var newItem in from i in e.NewItems.Cast<ISortableEntity>()
                         orderby i.SortOrder ascending
                         select i)
                {
                    newItem.SortOrder = nextSortOrder++;
                }

                if (searchForFollowingItems)
                {
                    foreach (var entry in from i in collection.Cast<ISortableEntity>()
                             where i.SortOrder >= startSortOrder
                             orderby i.SortOrder ascending
                             select i)
                    {
                        if (e.NewItems.Contains(entry))
                            continue; // skip all new entries

                        var newSortOrder = nextSortOrder++;
                        if (entry.SortOrder >= newSortOrder)
                            return; // there was a gap in the sort order which has been filled with the new items, we don't need to go through the rest of the list

                        entry.SortOrder = newSortOrder;
                    }
                }
            }
                break;
            case NotifyCollectionChangedAction.Move:
            {
                if (!(collection is IList<TEntry> list))
                    throw new NotSupportedException($"NotifyCollectionChangedAction.Move is not supported from {nameof(SortableDataSetView)} when collection does not implement IList<T>");

                var leftItem = e.OldItems?[0] as ISortableEntity;
                var rightItem = list[e.OldStartingIndex] as ISortableEntity;
                Debug.Assert(leftItem != null, nameof(leftItem) + " != null");
                Debug.Assert(rightItem != null, nameof(rightItem) + " != null");

                var sortOrderCache = rightItem.SortOrder;
                rightItem.SortOrder = leftItem.SortOrder;
                leftItem.SortOrder = sortOrderCache;
            }
                break;
            case NotifyCollectionChangedAction.Remove:
                return; // we don't change anything here because it will work nevertheless
            case NotifyCollectionChangedAction.Replace:
            {
                throw new NotSupportedException(
                    $"NotifyCollectionChangedAction.Replace is not supported from {nameof(SortableDataSetView)}");
            }
            case NotifyCollectionChangedAction.Reset:
                return;
            default:
                throw new NotSupportedException();
        }
    }
}