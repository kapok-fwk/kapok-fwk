namespace Kapok.Acl;

/// <summary>
/// A authentication service supporting the option to sign in silent.
///
/// When silent authentication failed, you can still call <see cref="IAuthenticationService.Login"/>.
/// </summary>
public interface ISilentAuthenticationService : IAuthenticationService
{
    /// <summary>
    /// Tries to log in silently.
    /// </summary>
    /// <returns>
    /// True if silent login was successfully, otherwise false.
    /// </returns>
    Task<bool> SilentLogin();
}