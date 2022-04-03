namespace Kapok.Acl;

/// <summary>
/// An interface handling the authentication for a user.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// A unique name of the authentication service.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Performs the authentication to the authentication service.
    /// </summary>
    Task Login();

    /// <summary>
    /// Performs a log out.
    /// </summary>
    Task Logout();

    /// <summary>
    /// If user login was successful and the authentication provider
    /// has access to the user name, this property will give back the
    /// user name of the user logged in.
    ///
    /// This can be used for displaying the users name.
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// If user login was successful and the authentication provider
    /// has access to the user email, this property will give back the
    /// user email of the user logged in.
    ///
    /// This can be helpful to unique identify a registered user
    /// without having to register the users internal account id.
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// If user login was successful, this returns a unique id identifying
    /// the user e.g. an email address, GUID or an S-ID.
    /// </summary>
    string? UserAccountId { get; }
}