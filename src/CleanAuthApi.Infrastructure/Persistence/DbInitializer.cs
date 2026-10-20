using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CleanAuthApi.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task InitializeDatabaseAsync(IServiceProvider services, IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "SqlServer";
        if (provider.Equals("Testing", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            if (provider.StartsWith("Postgre", StringComparison.OrdinalIgnoreCase))
            {
                const string createTablesSql = """
                    -- 1. Users Tablosu
                    CREATE TABLE IF NOT EXISTS public."Users" (
                        "Id" uuid NOT NULL,
                        "Email" character varying(256) NOT NULL,
                        "PasswordHash" text NOT NULL,
                        "FirstName" character varying(100) NOT NULL,
                        "LastName" character varying(100) NOT NULL,
                        "Role" character varying(50) NOT NULL DEFAULT 'User',
                        "IsActive" boolean NOT NULL DEFAULT TRUE,
                        "RefreshToken" character varying(256) NULL,
                        "RefreshTokenExpiryTime" timestamp with time zone NULL,
                        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
                        "UpdatedAt" timestamp with time zone NULL,
                        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
                    );
                    CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email" ON public."Users" ("Email");

                    -- 2. DatabaseErrorLogs Tablosu (Veritabanı Hata Logları)
                    CREATE TABLE IF NOT EXISTS public."DatabaseErrorLogs" (
                        "Id" uuid NOT NULL,
                        "ErrorType" character varying(200) NOT NULL,
                        "ErrorCode" character varying(100) NULL,
                        "Message" text NOT NULL,
                        "StackTrace" text NULL,
                        "SourceOperation" character varying(256) NULL,
                        "UserId" uuid NULL,
                        "IpAddress" character varying(64) NULL,
                        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
                        "UpdatedAt" timestamp with time zone NULL,
                        CONSTRAINT "PK_DatabaseErrorLogs" PRIMARY KEY ("Id")
                    );
                    CREATE INDEX IF NOT EXISTS "IX_DatabaseErrorLogs_CreatedAt" ON public."DatabaseErrorLogs" ("CreatedAt");

                    -- 3. FailedLoginLogs Tablosu (Başarısız Giriş / Yanlış Şifre Logları)
                    CREATE TABLE IF NOT EXISTS public."FailedLoginLogs" (
                        "Id" uuid NOT NULL,
                        "Email" character varying(256) NOT NULL,
                        "UserId" uuid NULL,
                        "FailureReason" character varying(256) NOT NULL,
                        "IpAddress" character varying(64) NULL,
                        "UserAgent" character varying(512) NULL,
                        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
                        "UpdatedAt" timestamp with time zone NULL,
                        CONSTRAINT "PK_FailedLoginLogs" PRIMARY KEY ("Id")
                    );
                    CREATE INDEX IF NOT EXISTS "IX_FailedLoginLogs_Email" ON public."FailedLoginLogs" ("Email");
                    CREATE INDEX IF NOT EXISTS "IX_FailedLoginLogs_CreatedAt" ON public."FailedLoginLogs" ("CreatedAt");

                    -- 4. PasswordChangeLogs Tablosu (Şifre Değişikliği Deneme ve Başarı Logları)
                    CREATE TABLE IF NOT EXISTS public."PasswordChangeLogs" (
                        "Id" uuid NOT NULL,
                        "UserId" uuid NOT NULL,
                        "Email" character varying(256) NOT NULL,
                        "IsSuccess" boolean NOT NULL,
                        "FailureReason" character varying(256) NULL,
                        "IpAddress" character varying(64) NULL,
                        "UserAgent" character varying(512) NULL,
                        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
                        "UpdatedAt" timestamp with time zone NULL,
                        CONSTRAINT "PK_PasswordChangeLogs" PRIMARY KEY ("Id")
                    );
                    CREATE INDEX IF NOT EXISTS "IX_PasswordChangeLogs_UserId" ON public."PasswordChangeLogs" ("UserId");
                    CREATE INDEX IF NOT EXISTS "IX_PasswordChangeLogs_CreatedAt" ON public."PasswordChangeLogs" ("CreatedAt");

                    -- 5. SecurityAuditLogs Tablosu (Genel Güvenlik Olay Logları)
                    CREATE TABLE IF NOT EXISTS public."SecurityAuditLogs" (
                        "Id" uuid NOT NULL,
                        "UserId" uuid NULL,
                        "Email" character varying(256) NULL,
                        "EventType" character varying(100) NOT NULL,
                        "Description" character varying(500) NOT NULL,
                        "IpAddress" character varying(64) NULL,
                        "UserAgent" character varying(512) NULL,
                        "CreatedAt" timestamp with time zone NOT NULL DEFAULT (now() AT TIME ZONE 'utc'),
                        "UpdatedAt" timestamp with time zone NULL,
                        CONSTRAINT "PK_SecurityAuditLogs" PRIMARY KEY ("Id")
                    );
                    CREATE INDEX IF NOT EXISTS "IX_SecurityAuditLogs_EventType" ON public."SecurityAuditLogs" ("EventType");
                    CREATE INDEX IF NOT EXISTS "IX_SecurityAuditLogs_CreatedAt" ON public."SecurityAuditLogs" ("CreatedAt");
                    """;

                await context.Database.ExecuteSqlRawAsync(createTablesSql);
                logger.LogInformation("PostgreSQL kullanıcı ve güvenlik log tabloları başarıyla kontrol edildi/oluşturuldu.");
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Veritabanı başlatılırken bir hata oluştu: {Message}", ex.Message);
        }
    }
}
