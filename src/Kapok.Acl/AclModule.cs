using Kapok.Core;
using Kapok.Acl.DataModel;

namespace Kapok.Acl;

public sealed class AclModule : ModuleBase
{
    public AclModule() : base(nameof(AclModule))
    {
    }

    public override void Initiate()
    {
        base.Initiate();

        // register DataModel
        DataDomain.RegisterEntity<RoleClaim, RoleClaimDao>();
        DataDomain.RegisterEntity<Role, RoleDao>();
        DataDomain.RegisterEntity<UserLogin, UserLoginDao>();
        DataDomain.RegisterEntity<User, UserDao>();
        DataDomain.RegisterEntity<UserRole, UserRoleDao>();
    }
}