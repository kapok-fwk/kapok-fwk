using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;

namespace Kapok.View.DataModel;

[Table(nameof(PageListView), Schema = "System")]
public class PageListView : EditableEntityBase
{
    static PageListView()
    {
        RegisterModel<PageListView>(entity =>
        {
            entity.AddOneToManyRelationship<Page>()
                .HasForeignKey(nameof(PageId));
        });
    }

    private Guid _pageId;
    private string _name = string.Empty;
    private string? _data;

    [Key]
    public Guid PageId
    {
        get => _pageId;
        set => SetProperty(ref _pageId, value);
    }

    /// <summary>
    /// The name of the list view.
    /// </summary>
    [Key]
    [StringLength(128)]
    public string Name
    {
        get => _name;
        set => SetValidateProperty(ref _name, value);
    }

    /// <summary>
    /// JSON object holding a serialized <see cref="DataSetListView"/>.
    /// </summary>
    public string? Data
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }
}