using Kapok.Acl.DataModel;
using Kapok.Acl.BusinessLayer;
using Kapok.Data;
using Kapok.Module;

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
        DataDomain.RegisterEntity<LoginProvider, LoginProviderService>();
        DataDomain.RegisterEntity<RoleClaim, RoleClaimService>();
        DataDomain.RegisterEntity<Role, RoleService>();
        DataDomain.RegisterEntity<UserLogin, UserLoginService>();
        DataDomain.RegisterEntity<User, UserService>();
        DataDomain.RegisterEntity<UserRole, UserRoleService>();
    }
}