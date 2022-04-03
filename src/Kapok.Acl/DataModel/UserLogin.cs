using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kapok.Entity;
using Res = Kapok.Acl.Resources.UserLogin;

namespace Kapok.Acl.DataModel;

[Table("UserLogin", Schema = "System")]
public class UserLogin : EditableEntityBase, IEquatable<UserLogin>
{
    static UserLogin()
    {
        RegisterModel<UserLogin>(entity =>
        {
            entity.AddOneToManyRelationship<User>(nameof(User))
                .HasForeignKey(nameof(UserId))
                .WithForeignNavigationProperty(nameof(DataModel.User.Logins));

            entity.AddUniqueIndex(nameof(LoginProvider), nameof(ProviderKey));
        });
    }

    private byte[]? _rowVersion;
    private Guid _userId;
    private string _loginProvider = string.Empty;
    private string _providerKey = string.Empty;

    [Timestamp]
    [Browsable(false)]
    public byte[]? RowVersion
    {
        get => _rowVersion;
        set => SetProperty(ref _rowVersion, value);
    }

    /// <summary>
    /// Gets or sets the of the primary key of the user associated with this login.
    /// </summary>
    [Key]
    [Browsable(false)]
    [Display(Name = "UserId", ResourceType = typeof(Res))]
    public Guid UserId
    {
        get => _userId;
        set => SetValidateProperty(ref _userId, value);
    }

    public virtual User? User { get; set; }

    /// <summary>
    /// Gets or sets the unique provider identifier for this login.
    /// </summary>
    [Key]
    [StringLength(50)]
    [Display(Name = "LoginProvider", ResourceType = typeof(Res))]
    [LookupColumn]
    public string LoginProvider
    {
        get => _loginProvider;
        set => SetValidateProperty(ref _loginProvider, value);
    }

    /// <summary>
    /// Gets or sets the unique provider identifier for this login.
    /// </summary>
    // Note: a windows SID has a max length of 184 chars (source: https://stackoverflow.com/questions/1140528/what-is-the-maximum-length-of-a-sid-in-sddl-format)
    [StringLength(255)]
    [Display(Name = "ProviderKey", ResourceType = typeof(Res))]
    public string ProviderKey
    {
        get => _providerKey;
        set => SetValidateProperty(ref _providerKey, value);
    }

        
    #region IEquatable<User>

    public bool Equals(UserLogin? other)
    {
        if (other == null) return false;

        return UserId == other.UserId && LoginProvider == other.LoginProvider;
    }

    #endregion
}