using Kapok.Acl;
using Microsoft.AspNetCore.Authorization;
#pragma warning disable 1591

namespace Kapok.AspNetCore.Authorization;

public class PermissionRequirement : IAuthorizationRequirement
{
    public ClaimType ClaimType { get; }

    public string ClaimValue { get; }

    public PermissionRequirement(ClaimType claimType, string claimValue)
    {
        ClaimType = claimType;
        ClaimValue = claimValue;
    }

    public override string ToString()
    {
        return $"{ClaimType} {ClaimValue}";
    }
}