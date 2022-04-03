using System.ComponentModel.DataAnnotations;
using Kapok.Core;
using Kapok.Entity;
using Res = Kapok.View.Resources.HierarchyListPage;

namespace Kapok.View;

/// <summary>
/// A base class for a hierarchy list page.
/// </summary>
/// <typeparam name="TEntry"></typeparam>
public abstract class HierarchyListPage<TEntry> : ListPage<TEntry>, IHierarchyListPage<TEntry>
    where TEntry : class, IHierarchyEntry<TEntry>, new()
{
    public HierarchyListPage(IViewDomain? viewDomain, IDataDomainScope? dataDomainScope = null)
        : base(viewDomain, dataDomainScope)
    {
        MoveInEntryAction = new UIAction(
            "MoveInEntry",
            () => (DataSet as IHierarchyDataSetView<TEntry>)?.MoveInAction.Execute(),
            () => (DataSet as IHierarchyDataSetView<TEntry>)?.MoveInAction.CanExecute() ?? false
        ) {Image = "table-row-insert"};
        MoveOutEntryAction = new UIAction(
            "MoveOutEntry",
            () => (DataSet as IHierarchyDataSetView<TEntry>)?.MoveOutAction.Execute(),
            () => (DataSet as IHierarchyDataSetView<TEntry>)?.MoveOutAction.CanExecute() ?? false
        ) {Image = "table-row-extract"};
    }

    // ReSharper disable MemberCanBePrivate.Global
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    [MenuItem, Display(Name = "MoveInEntryCommand_Name", Description = "MoveInEntryCommand_Description", GroupName = "Manage", ResourceType = typeof(Res))]
    public IAction MoveInEntryAction { get; }
        
    [MenuItem, Display(Name = "MoveOutEntryCommand_Name", Description = "MoveOutEntryCommand_Description", GroupName = "Manage", ResourceType = typeof(Res))]
    public IAction MoveOutEntryAction { get; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
    // ReSharper restore MemberCanBePrivate.Global
}