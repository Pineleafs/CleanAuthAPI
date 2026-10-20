using CleanAuthApi.Application.Interfaces;
using CleanAuthApi.Domain.Interfaces;
using CleanAuthApi.Infrastructure.Persistence;
using CleanAuthApi.Infrastructure.Persistence.Repositories;
using CleanAuthApi.Infrastructure.Security;
using CleanAuthApi.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanAuthApi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "SqlServer";

        if (!provider.Equals("Testing", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                if (provider.StartsWith("Postgre", StringComparison.OrdinalIgnoreCase))
                {
                    var pgConnection = configuration.GetConnectionString("PostgreSQL")
                        ?? throw new InvalidOperationException("PostgreSQL bağlantı cümlesi (ConnectionStrings:PostgreSQL) bulunamadı.");
                    options.UseNpgsql(pgConnection);
                }
                else
                {
                    var sqlConnection = configuration.GetConnectionString("SqlServer")
                        ?? throw new InvalidOperationException("SQL Server bağlantı cümlesi (ConnectionStrings:SqlServer) bulunamadı.");
                    options.UseSqlServer(sqlConnection);
                }
            });
        }

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISecurityLogService, SecurityLogService>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        return services;
    }
}
