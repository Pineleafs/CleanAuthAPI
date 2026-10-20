using CleanAuthApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CleanAuthApi.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<DatabaseErrorLog> DatabaseErrorLogs => Set<DatabaseErrorLog>();
    public DbSet<FailedLoginLog> FailedLoginLogs => Set<FailedLoginLog>();
    public DbSet<PasswordChangeLog> PasswordChangeLogs => Set<PasswordChangeLog>();
    public DbSet<SecurityAuditLog> SecurityAuditLogs => Set<SecurityAuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
