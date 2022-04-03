namespace Kapok.Acl;

public class AnonymousIdentity : UserIdentity
{
    public AnonymousIdentity()
        : base(displayName: "Anonymous",
            loginProviderName: null,
            isAuthenticated: false)
    {
    }
}