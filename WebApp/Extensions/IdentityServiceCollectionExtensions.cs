using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WebApp.Extensions;

public static class IdentityServiceCollectionExtensions
{
    public static IdentityBuilder AddBaseIdentity<TUser, TRole>(
        this IServiceCollection services,
        Action<IdentityOptions>? setupAction)
        where TUser : class
        where TRole : class
    {
        // Hosting doesn't add IHttpContextAccessor by default
        services.AddHttpContextAccessor();
        // Identity services
        services.AddScoped<IUserValidator<TUser>, UserValidator<TUser>>();
        services.AddScoped<IPasswordValidator<TUser>, PasswordValidator<TUser>>();
        services.AddScoped<IPasswordHasher<TUser>, PasswordHasher<TUser>>();
        services.AddScoped<ILookupNormalizer, UpperInvariantLookupNormalizer>();
        services.AddScoped<IRoleValidator<TRole>, RoleValidator<TRole>>();
        // No interface for the error describer so we can add errors without rev'ing the interface
        services.AddScoped<IdentityErrorDescriber>();
        services.AddScoped<IUserClaimsPrincipalFactory<TUser>, UserClaimsPrincipalFactory<TUser, TRole>>();
        services.AddScoped<IUserConfirmation<TUser>, DefaultUserConfirmation<TUser>>();
        services.AddScoped<UserManager<TUser>>();
        services.AddScoped<RoleManager<TRole>>();

        if (setupAction != null)
        {
            services.Configure(setupAction);
        }

        return new IdentityBuilder(typeof(TUser), typeof(TRole), services);
    }
}