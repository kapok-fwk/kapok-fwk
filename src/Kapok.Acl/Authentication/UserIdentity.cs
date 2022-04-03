using System.Security.Principal;

namespace Kapok.Acl;

// see:
// - https://blog.magnusmontin.net/2013/03/24/custom-authorization-in-wpf/
// - https://social.technet.microsoft.com/wiki/contents/articles/25726.wpf-implementing-custom-authentication-and-authorization.aspx
public abstract class UserIdentity : IIdentity
{
    protected UserIdentity(string displayName, string? loginProviderName, bool isAuthenticated)
    {
        DisplayName = displayName;
        LoginProviderName = loginProviderName;
        IsAuthenticated = isAuthenticated;
    }

    public string DisplayName { get; }

    public string? LoginProviderName { get; }

    public bool IsAuthenticated { get; }

    #region IIdentity Members

    string IIdentity.Name => DisplayName;

    string IIdentity.AuthenticationType => LoginProviderName;

    #endregion
}