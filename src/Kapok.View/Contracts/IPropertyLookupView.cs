using Kapok.Entity.Model;

namespace Kapok.View;

/// <summary>
/// A property lookup view holds the information what possible options to show in a UI combobox.
/// </summary>
public interface IPropertyLookupView
{
    /// <summary>
    /// Holds the lookup definition.
    /// </summary>
    ILookupDefinition LookupDefinition { get; }

    /// <summary>
    /// Tell to refresh the list of possible options in the combobox menu.
    ///
    /// This is e.g. called when possible options depend on a property of the current row and
    /// the current row changed.
    /// </summary>
    void Refresh();
}