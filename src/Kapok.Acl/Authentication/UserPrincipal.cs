using System.Security.Principal;

namespace Kapok.Acl;

public sealed class UserPrincipal : IPrincipal
{
    private UserPrincipal()
    {
    }

    private static UserPrincipal? _singleton;

    public static UserPrincipal Singleton => _singleton ??= new UserPrincipal();


    private UserIdentity? _identity;

    public UserIdentity Identity
    {
        get => _identity ?? new AnonymousIdentity();
        set => _identity = value;
    }

    #region IPrincipal Members

    IIdentity IPrincipal.Identity => Identity;

    public bool IsInRole(string role)
    {
        if (Identity is DbUserIdentity dbUserIdentity)
        {
            return dbUserIdentity.IsInRole(role);
        }

        return false;
    }

    #endregion
}