using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;
using Res = Kapok.Acl.Resources.UserRole;

namespace Kapok.Acl.DataModel;

[Table(nameof(UserRole), Schema = "System")]
public class UserRole : EditableEntityBase, IEquatable<UserRole>
{
    static UserRole()
    {
        RegisterModel<UserRole>(entity =>
        {
            entity.AddOneToManyRelationship<User>(nameof(User))
                .HasForeignKey(nameof(UserId))
                .WithForeignNavigationProperty(nameof(DataModel.User.Roles));
            entity.AddOneToManyRelationship<Role>(nameof(Role))
                .HasForeignKey(nameof(RoleId));
        });
    }

    private byte[]? _rowVersion;
    private Guid _userId;
    private Guid _roleId;

    [Timestamp]
    [Browsable(false)]
    public byte[]? RowVersion
    {
        get => _rowVersion;
        set => SetProperty(ref _rowVersion, value);
    }

    [Key]
    [Browsable(false)]
    [Display(Name = nameof(UserId), ResourceType = typeof(Res))]
    public Guid UserId
    {
        get => _userId;
        set => SetValidateProperty(ref _userId, value);
    }

    public virtual User? User { get; set; }

    [Key]
    [Browsable(false)]
    [Display(Name = nameof(RoleId), ResourceType = typeof(Res))]
    public Guid RoleId
    {
        get => _roleId;
        set => SetValidateProperty(ref _roleId, value);
    }

    public virtual Role? Role { get; set; }

    #region IEquatable<UserRole>

    public bool Equals(UserRole? other)
    {
        if (other == null) return false;

        return UserId == other.UserId &&
               RoleId == other.RoleId;
    }

    #endregion
}