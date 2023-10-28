using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;

namespace Kapok.View.DataModel;

[Table(nameof(Page), Schema = "System")]
public class Page : EditableEntityBase
{
    private Guid _pageId;
    private string _typeFullName = string.Empty;

    [Key]
    [AutoGenerateValue(AutoGenerateValueType.Identity)]
    public Guid PageId
    {
        get => _pageId;
        set => SetProperty(ref _pageId, value);
    }

    [StringLength(512)]
    public string TypeFullName
    {
        get => _typeFullName;
        set => SetValidateProperty(ref _typeFullName, value);
    }
}