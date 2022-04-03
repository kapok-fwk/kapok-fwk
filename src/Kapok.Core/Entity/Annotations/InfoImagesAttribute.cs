using System.ComponentModel.DataAnnotations.Schema;

namespace Kapok.Entity;

/// <summary>
/// Usable for entity properties with the type
///
/// ObservableCollection&lt;string&gt;
///
/// containing a list of images which shall be shown to the user.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class InfoImagesAttribute : NotMappedAttribute
{
}