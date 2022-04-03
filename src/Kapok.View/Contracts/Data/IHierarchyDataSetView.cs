using Kapok.Entity;

namespace Kapok.View;

public interface IHierarchyDataSetView<TEntry> : IDataSetView<TEntry>
    where TEntry : class, IHierarchyEntry<TEntry>, new()
{
    IAction ExpandAction { get; }
    IAction CollapseAction { get; }

    IAction ToggleAction { get; }

    IAction MoveInAction { get; }
    IAction MoveOutAction { get; }
}