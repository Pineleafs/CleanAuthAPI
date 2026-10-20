using CleanAuthApi.Application.Interfaces;
using CleanAuthApi.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CleanAuthApi.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
