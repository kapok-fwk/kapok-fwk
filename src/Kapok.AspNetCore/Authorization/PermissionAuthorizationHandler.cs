using Microsoft.AspNetCore.Authorization;
#pragma warning disable 1591

namespace Kapok.AspNetCore.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement, string>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
        PermissionRequirement requirement,
        string resource)
    {
        if (context.User == null || resource == null)
        {
            return Task.CompletedTask;
        }

        // TODO: today we don't use the 'resource' value; but in the future this can be used to restrict access on specific entries
        if (context.User.HasClaim(requirement.ClaimType.ToString(), requirement.ClaimValue))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}