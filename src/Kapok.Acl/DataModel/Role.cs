using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;
using Res = Kapok.Acl.Resources.Role;

namespace Kapok.Acl.DataModel;

[Table(nameof(Role), Schema = "System")]
public class Role : EditableEntityBase, IEquatable<Role>
{
    static Role()
    {
        RegisterModel<Role>(entity =>
        {
            entity.SetPrimaryKey(nameof(Id));

            entity.AddManyToOneRelationship<RoleClaim>(nameof(RoleClaims))
                .WithForeignNavigationProperty(nameof(RoleClaim.Role));
        });
    }

    private byte[]? _rowVersion;
    private Guid _id;
    private string _name = string.Empty;

    [Timestamp]
    [Browsable(false)]
    public byte[]? RowVersion
    {
        get => _rowVersion;
        set => SetProperty(ref _rowVersion, value);
    }

    [Browsable(false)]
    [Display(Name = nameof(Id), ResourceType = typeof(Res))]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id
    {
        get => _id;
        set => SetValidateProperty(ref _id, value);
    }

    [Required(AllowEmptyStrings = false)]
    [StringLength(80)]
    [Display(Name = nameof(Name), ResourceType = typeof(Res))]
    [LookupColumn]
    public string Name
    {
        get => _name;
        set => SetValidateProperty(ref _name, value);
    }

    public virtual List<RoleClaim>? RoleClaims { get; set; }

    #region IEquatable<Role>

    public bool Equals(Role? other)
    {
        if (other == null) return false;

        return Id == other.Id;
    }

    #endregion
}