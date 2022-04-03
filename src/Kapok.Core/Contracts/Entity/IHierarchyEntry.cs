using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Kapok.Entity;

/// <summary>
/// Must be implemented from hierarchy entities.
/// 
/// INotifyPropertyChanged is implemented because it is required to be implemented for the properties of this interface.
/// </summary>
public interface IHierarchyEntry<TEntry> : INotifyPropertyChanged
    where TEntry : class, IHierarchyEntry<TEntry>
{
    /// <summary>
    /// returns the parent object of the hierarchy.
    /// 
    /// the set element is used to change the list parent. It is expected that a change will change the database parent field as well.
    /// </summary>
    TEntry Parent { get; set; }

    /// <summary>
    /// Entry for the GUI, is optional in the dataset.
    /// 
    /// Gives the current hierarchy level starting with zero.
    /// </summary>
    int Level { get; set; }

    /// <summary>
    /// Entry for the GUI, not required in the dataset.
    /// 
    /// Gives or sets the information if the current entry is expanded so its child entries are shown.
    /// </summary>
    [NotMapped, JsonIgnore]
    bool IsExpanded { get; set; }

    /// <summary>
    /// Entry for the GUI, not required in the dataset.
    /// 
    /// Gives or sets the information if the current entry is visible.
    /// Level zero elements should always be visible.
    /// </summary>
    [NotMapped, JsonIgnore]
    bool IsVisible { get; set; }

    /// <summary>
    /// Entry for the GUI, not required in the dataset.
    /// 
    /// Gives or sets the information if the current entry has child elements.
    /// </summary>
    [NotMapped, JsonIgnore]
    bool HasChildren { get; set; }

    // TODO: this causes problems today, see https://github.com/aspnet/EntityFrameworkCore/issues/13391
    //[NotMapped]
    //List<TEntry> Children { get; set; }

    /// <summary>
    /// Returns a predicate which can be used to filter a collection
    /// for the children of the current record.
    /// </summary>
    /// <returns></returns>
    Func<TEntry, bool> GetChildrenPredicate();
}