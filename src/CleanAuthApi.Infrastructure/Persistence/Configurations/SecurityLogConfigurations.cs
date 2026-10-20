using CleanAuthApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanAuthApi.Infrastructure.Persistence.Configurations;

public class DatabaseErrorLogConfiguration : IEntityTypeConfiguration<DatabaseErrorLog>
{
    public void Configure(EntityTypeBuilder<DatabaseErrorLog> builder)
    {
        builder.ToTable("DatabaseErrorLogs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.ErrorType).IsRequired().HasMaxLength(200);
        builder.Property(e => e.ErrorCode).HasMaxLength(100);
        builder.Property(e => e.Message).IsRequired();
        builder.Property(e => e.SourceOperation).HasMaxLength(256);
        builder.Property(e => e.IpAddress).HasMaxLength(64);

        builder.HasIndex(e => e.CreatedAt);
    }
}

public class FailedLoginLogConfiguration : IEntityTypeConfiguration<FailedLoginLog>
{
    public void Configure(EntityTypeBuilder<FailedLoginLog> builder)
    {
        builder.ToTable("FailedLoginLogs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.FailureReason).IsRequired().HasMaxLength(256);
        builder.Property(e => e.IpAddress).HasMaxLength(64);
        builder.Property(e => e.UserAgent).HasMaxLength(512);

        builder.HasIndex(e => e.Email);
        builder.HasIndex(e => e.CreatedAt);
    }
}

public class PasswordChangeLogConfiguration : IEntityTypeConfiguration<PasswordChangeLog>
{
    public void Configure(EntityTypeBuilder<PasswordChangeLog> builder)
    {
        builder.ToTable("PasswordChangeLogs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.FailureReason).HasMaxLength(256);
        builder.Property(e => e.IpAddress).HasMaxLength(64);
        builder.Property(e => e.UserAgent).HasMaxLength(512);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.CreatedAt);
    }
}

public class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
{
    public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
    {
        builder.ToTable("SecurityAuditLogs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email).HasMaxLength(256);
        builder.Property(e => e.EventType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(500);
        builder.Property(e => e.IpAddress).HasMaxLength(64);
        builder.Property(e => e.UserAgent).HasMaxLength(512);

        builder.HasIndex(e => e.EventType);
        builder.HasIndex(e => e.CreatedAt);
    }
}
