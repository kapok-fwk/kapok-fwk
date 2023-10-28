using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;

namespace Kapok.View.DataModel;

[Table(nameof(UserPageListView), Schema = "System")]
public class UserPageListView : EditableEntityBase
{
    static UserPageListView()
    {
        RegisterModel<UserPageListView>(entity =>
        {
            entity.AddOneToManyRelationship<Page>()
                .HasForeignKey(nameof(PageId));
        });
    }

    private Guid _userId;
    private Guid _pageId;
    private string _name = string.Empty;
    private string _data = string.Empty;

    /// <summary>
    /// The user id holding the user specific individualization for this list view.
    /// </summary>
    [Key]
    public Guid UserId
    {
        get => _userId;
        set => SetProperty(ref _userId, value);
    }

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
    public string Data
    {
        get => _data;
        set => SetValidateProperty(ref _data, value);
    }
}