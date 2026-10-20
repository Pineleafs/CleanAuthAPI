-- ============================================================================
-- CleanAuthApi - PostgreSQL Tüm Tablolar ve Güvenlik Logları (DDL)
-- Veritabanı: datarotasosyaldb
-- ============================================================================

-- 1. KULLANICILAR TABLOSU (Users)
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

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Users_Email"
    ON public."Users" ("Email");


-- 2. VERİTABANI VE SİSTEM HATA LOGLARI TABLOSU (DatabaseErrorLogs)
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

CREATE INDEX IF NOT EXISTS "IX_DatabaseErrorLogs_CreatedAt"
    ON public."DatabaseErrorLogs" ("CreatedAt");


-- 3. BAŞARISIZ GİRİŞ / YANLIŞ ŞİFRE LOGLARI TABLOSU (FailedLoginLogs)
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

CREATE INDEX IF NOT EXISTS "IX_FailedLoginLogs_Email"
    ON public."FailedLoginLogs" ("Email");

CREATE INDEX IF NOT EXISTS "IX_FailedLoginLogs_CreatedAt"
    ON public."FailedLoginLogs" ("CreatedAt");


-- 4. ŞİFRE DEĞİŞİKLİĞİ LOGLARI TABLOSU (PasswordChangeLogs)
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

CREATE INDEX IF NOT EXISTS "IX_PasswordChangeLogs_UserId"
    ON public."PasswordChangeLogs" ("UserId");

CREATE INDEX IF NOT EXISTS "IX_PasswordChangeLogs_CreatedAt"
    ON public."PasswordChangeLogs" ("CreatedAt");


-- 5. GENEL GÜVENLİK DENETİM LOGLARI TABLOSU (SecurityAuditLogs)
-- (Kayıt, Başarılı Giriş, Token Yenileme, Oturum Kapatma, Profil Güncelleme, Hesap Silme)
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

CREATE INDEX IF NOT EXISTS "IX_SecurityAuditLogs_EventType"
    ON public."SecurityAuditLogs" ("EventType");

CREATE INDEX IF NOT EXISTS "IX_SecurityAuditLogs_CreatedAt"
    ON public."SecurityAuditLogs" ("CreatedAt");
