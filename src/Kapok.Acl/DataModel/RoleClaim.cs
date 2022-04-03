using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;
using Res = Kapok.Acl.Resources.RoleClaim;

namespace Kapok.Acl.DataModel;

[Table("RoleClaim", Schema = "System")]
public class RoleClaim : EditableEntityBase, IEquatable<RoleClaim>
{
    static RoleClaim()
    {
        RegisterModel<RoleClaim>(entity =>
        {
            entity.AddOneToManyRelationship<Role>(nameof(Role))
                .HasForeignKey(nameof(RoleId))
                .WithForeignNavigationProperty(nameof(DataModel.Role.RoleClaims));
        });
    }

    private byte[]? _rowVersion;
    private string _claimType = string.Empty;
    private string _claimValue;
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
    [Display(Name = "RoleId", ResourceType = typeof(Res))]
    public Guid RoleId
    {
        get => _roleId;
        set => SetValidateProperty(ref _roleId, value);
    }

    public virtual Role? Role { get; set; }

    [Key]
    [StringLength(10)]
    [Display(Name = "ClaimType", ResourceType = typeof(Res))]
    [LookupColumn]
    public string ClaimType
    {
        get => _claimType;
        set => SetValidateProperty(ref _claimType, value);
    }

    [Key]
    [StringLength(255)]
    [Display(Name = "ClaimValue", ResourceType = typeof(Res))]
    [LookupColumn]
    public string ClaimValue
    {
        get => _claimValue;
        set => SetValidateProperty(ref _claimValue, value);
    }

        
    #region IEquatable<RoleClaim>

    public bool Equals(RoleClaim? other)
    {
        if (other == null) return false;

        return RoleId == other.RoleId && ClaimType == other.ClaimType && ClaimValue == other.ClaimValue;
    }

    #endregion
}