using System.Diagnostics;
using Kapok.Acl.DataModel;
using Res = Kapok.Acl.Resources.DbUserIdentity;

namespace Kapok.Acl;

public class DbUserIdentity : UserIdentity
{
    public UserLogin DbUserLogin { get; }

#pragma warning disable CS8603
    public User DbUser => DbUserLogin.User;
#pragma warning restore CS8603

    public DbUserIdentity(UserLogin userLogin)
        : base(userLogin.User?.UserName ?? string.Empty, userLogin.LoginProvider, isAuthenticated: true)
    {
        if (userLogin.User == null)
            throw new ArgumentException(
                string.Format(Res.Constructor_UserNotGiven, nameof(userLogin), nameof(UserLogin.User)));

        DbUserLogin = userLogin;
    }

    public bool IsInRole(string role)
    {
        if (DbUser.Roles == null)
            return false;

        foreach (var userRoles in DbUser.Roles)
        {
            Debug.Assert(userRoles.Role != null);
            Debug.Assert(userRoles.Role.RoleClaims != null);

            if (userRoles.Role.RoleClaims
                .Where(c => c.ClaimType == ClaimType.Function.ToString())
                .Any(claim => claim.ClaimValue == role))
            {
                return true;
            }
        }

        return false;
    }
}