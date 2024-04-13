using Kapok.Acl;
using Kapok.Acl.DataModel;
using Kapok.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Kapok.AspNetCore;

public static class ServiceExtensions
{
    public static void ConfigureAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationCore(options =>
        {
            options.AddPolicy("ClaimFromDbPolicy",
                policy =>
                {
                    policy.Requirements.Add(
                        // this is just a dummy record; we don't have to list here each object which is allowed.
                        new PermissionRequirement(ClaimType.Function, null)
                    );
                });
        });

        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
    }

    public static void ConfigureIdentityModel(this IServiceCollection services)
    {
        services.AddIdentity<User, Role>().AddDefaultTokenProviders();
        services.AddTransient<IUserStore<User>, UserStore>();
        services.AddTransient<IRoleStore<Role>, RoleStore>();
    }
}