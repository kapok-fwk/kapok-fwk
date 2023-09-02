using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;
using Res = Kapok.Acl.Resources.User;

namespace Kapok.Acl.DataModel;

// NOTE: This class and its relatives is build on the basis of the identity model of ASP.NET
//       so that it will be easy to convert this in the future to an ASP.NET application.
//       
//       See also:
//         https://docs.microsoft.com/en-us/aspnet/core/security/authentication/customize_identity_model?view=aspnetcore-2.1
//
[Table(nameof(User), Schema = "System")]
public class User : EditableEntityBase, IEquatable<User>
{
    static User()
    {
        RegisterModel<User>(entity =>
        {
            entity.AddManyToOneRelationship<UserRole>(nameof(Roles))
                .WithForeignNavigationProperty(nameof(UserRole.User));

            entity.AddManyToOneRelationship<UserLogin>(nameof(Logins))
                .WithForeignNavigationProperty(nameof(UserLogin.User));
        });
    }

    private byte[]? _rowVersion;
    private Guid _id;
    private string _userName = string.Empty;

    [Timestamp]
    [Browsable(false)]
    public byte[]? RowVersion
    {
        get => _rowVersion;
        set => SetProperty(ref _rowVersion, value);
    }

    [Key]
    [Browsable(false)]
    [Display(Name = nameof(Id), ResourceType = typeof(Res))]
    public Guid Id
    {
        get => _id;
        set => SetValidateProperty(ref _id, value);
    }

    [StringLength(80)]
    [Display(Name = nameof(UserName), ResourceType = typeof(Res))]
    [LookupColumn]
    public string UserName
    {
        get => _userName;
        set => SetValidateProperty(ref _userName, value);
    }

    public virtual List<UserRole>? Roles { get; set; }

    public virtual List<UserLogin>? Logins { get; set; }

    #region IEquatable<User>

    public bool Equals(User? other)
    {
        if (other == null) return false;

        return Id == other.Id;
    }

    #endregion
}