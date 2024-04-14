using Kapok.Acl.DataModel;
using Kapok.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Kapok.AspNetCore.Authorization;

/// <summary>
/// 
/// </summary>
public class RoleStore : IRoleStore<Role>
{
    private readonly IDataDomainScope _dataDomainScope;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataDomainScope"></param>
    public RoleStore(IDataDomainScope dataDomainScope)
    {
        _dataDomainScope = dataDomainScope;
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
    /// <param name="role"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if(role == null)
            throw new ArgumentNullException(nameof(role));

        await _dataDomainScope.GetEntityService<Role>().CreateAsync(role);
        await _dataDomainScope.SaveAsync(cancellationToken);

        return IdentityResult.Success;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="role"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if(role == null)
            throw new ArgumentNullException(nameof(role));

        _dataDomainScope.GetEntityService<Role>().Update(role);
        await _dataDomainScope.SaveAsync(cancellationToken);

        return IdentityResult.Success;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="role"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if(role == null)
            throw new ArgumentNullException(nameof(role));

        _dataDomainScope.GetEntityService<Role>().Delete(role);
        await _dataDomainScope.SaveAsync(cancellationToken);

        return IdentityResult.Success;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="role"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await Task.FromResult(role.Id.ToString());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="role"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    async Task<string> IRoleStore<Role>.GetRoleNameAsync(Role role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await Task.FromResult(role.Name);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="role"></param>
    /// <param name="roleName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        role.Name = roleName;

        await Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="role"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await Task.FromResult(role.Name.ToUpper());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="role"></param>
    /// <param name="normalizedName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // do nothing

        await Task.CompletedTask;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="roleId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public async Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Guid.TryParse(roleId, out Guid roleIdAsGuid))
        {
            throw new ArgumentException($"The {roleId} parameter must be a GUID.", nameof(roleId));
        }

        return await _dataDomainScope.GetEntityService<Role>().AsQueryable().Where(r => r.Id == roleIdAsGuid).SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="normalizedRoleName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _dataDomainScope.GetEntityService<Role>().AsQueryable().Where(r => r.Name.ToUpper() == normalizedRoleName).SingleOrDefaultAsync(cancellationToken: cancellationToken);
    }
}