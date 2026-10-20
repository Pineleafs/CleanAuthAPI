using System.Data.Common;
using CleanAuthApi.Application.Interfaces;
using CleanAuthApi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CleanAuthApi.Infrastructure.Persistence.Repositories;

public class UnitOfWork(
    AppDbContext context,
    IUserRepository userRepository,
    ISecurityLogService securityLogService) : IUnitOfWork
{
    public IUserRepository Users { get; } = userRepository;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is DbUpdateException or DbException)
        {
            await securityLogService.LogDatabaseErrorAsync(ex, "UnitOfWork.SaveChangesAsync", cancellationToken);
            throw;
        }
    }

    public void Dispose()
    {
        context.Dispose();
        GC.SuppressFinalize(this);
    }
}
