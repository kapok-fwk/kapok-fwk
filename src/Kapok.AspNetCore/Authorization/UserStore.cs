using Kapok.Acl.DataModel;
using Kapok.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Kapok.AspNetCore.Authorization;

/// <summary>
/// 
/// </summary>
public class UserStore : IUserStore<User>
{
    private readonly IDataDomainScope _dataDomainScope;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataDomainScope"></param>
    public UserStore(IDataDomainScope dataDomainScope)
    {
        _dataDomainScope = dataDomainScope;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
            throw new ArgumentNullException(nameof(user));

        await _dataDomainScope.GetEntityService<User>().CreateAsync(user);
        await _dataDomainScope.SaveAsync(cancellationToken);

        return IdentityResult.Success;
    }
        
    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // validate record
        if (user == null)
        {
            return IdentityResult.Failed(new IdentityError {Code = "001", Description = "User record not given"});
        }
        if (string.IsNullOrEmpty(user.UserName))
        {
            return IdentityResult.Failed(new IdentityError {Code = "002", Description = "UserName must be filled."});
        }
        // TODO: add more validation for the 'User' record

        _dataDomainScope.GetEntityService<User>().Update(user);
        await _dataDomainScope.SaveAsync(cancellationToken);

        return IdentityResult.Success;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
            throw new ArgumentNullException(nameof(user));

        _dataDomainScope.GetEntityService<User>().Delete(user);
        await _dataDomainScope.SaveAsync(cancellationToken);

        return IdentityResult.Success;
    }

    /// <summary>
    /// 
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Guid.TryParse(userId, out Guid userIdAsGuid))
        {
            throw new ArgumentException($"The {userId} parameter must be a GUID.", nameof(userId));
        }
            
        return await _dataDomainScope.GetEntityService<User>().AsQueryable().Where(u => u.Id == userIdAsGuid)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="normalizedUserName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _dataDomainScope.GetEntityService<User>().AsQueryable().Where(u => u.UserName.ToUpper() == normalizedUserName)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    async Task<string> IUserStore<User>.GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await Task.FromResult(user.UserName.ToUpper());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await Task.FromResult(user.Id.ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await Task.FromResult(user.UserName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="normalizedName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // do nothing, see https://stackoverflow.com/questions/39651299/normalizedusername-vs-username-in-dotnet-core

        await Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="userName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        user.UserName = userName;

        await Task.CompletedTask;
    }
}