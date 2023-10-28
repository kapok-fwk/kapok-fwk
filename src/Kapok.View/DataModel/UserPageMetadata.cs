using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;

namespace Kapok.View.DataModel;

[Table(nameof(UserPageMetadata), Schema = "System")]
public class UserPageMetadata : EditableEntityBase
{
    static UserPageMetadata()
    {
        RegisterModel<UserPageMetadata>(entity =>
        {
            entity.AddOneToManyRelationship<Page>()
                .HasForeignKey(nameof(PageId));
        });
    }

    private Guid _userId;
    private Guid _pageId;
    private string _data = string.Empty;

    /// <summary>
    /// The user id holding the user specific individualization for this page.
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

    public string Data
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }
}