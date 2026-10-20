using CleanAuthApi.Application.Common.Exceptions;
using CleanAuthApi.Application.DTOs.Auth;
using CleanAuthApi.Application.Interfaces;
using CleanAuthApi.Domain.Entities;
using CleanAuthApi.Domain.Enums;
using CleanAuthApi.Domain.Interfaces;

namespace CleanAuthApi.Application.Services;

public class AuthService(
    IUnitOfWork unitOfWork,
    IJwtTokenGenerator jwtTokenGenerator,
    IPasswordHasher passwordHasher,
    ISecurityLogService securityLogService) : IAuthService
{
    private const int RefreshTokenExpiryDays = 7;

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (await unitOfWork.Users.ExistsByEmailAsync(normalizedEmail, cancellationToken))
        {
            await securityLogService.LogSecurityAuditAsync(
                null,
                normalizedEmail,
                "RegisterConflict",
                "Mevcut bir e-posta adresi ile tekrar kayıt denemesi yapıldı.",
                cancellationToken);

            throw new ConflictException("Bu e-posta adresi ile kayıtlı bir kullanıcı zaten mevcut.");
        }

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = passwordHasher.HashPassword(request.Password),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = UserRole.User,
            IsActive = true,
            RefreshToken = jwtTokenGenerator.GenerateRefreshToken(),
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays)
        };

        await unitOfWork.Users.AddAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await securityLogService.LogSecurityAuditAsync(
            user.Id,
            user.Email,
            "UserRegistered",
            "Yeni kullanıcı kaydı başarıyla oluşturuldu.",
            cancellationToken);

        var (accessToken, expiresAt) = jwtTokenGenerator.GenerateAccessToken(user);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            accessToken,
            user.RefreshToken,
            expiresAt);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await unitOfWork.Users.GetByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            await securityLogService.LogFailedLoginAsync(
                normalizedEmail,
                null,
                "Kayıtlı olmayan e-posta adresi ile giriş denemesi.",
                cancellationToken);

            throw new UnauthorizedAppException("Geçersiz e-posta adresi veya şifre.");
        }

        if (!passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            await securityLogService.LogFailedLoginAsync(
                normalizedEmail,
                user.Id,
                "Hatalı şifre girildi.",
                cancellationToken);

            throw new UnauthorizedAppException("Geçersiz e-posta adresi veya şifre.");
        }

        if (!user.IsActive)
        {
            await securityLogService.LogFailedLoginAsync(
                normalizedEmail,
                user.Id,
                "Pasif durumdaki hesap ile giriş denemesi.",
                cancellationToken);

            throw new UnauthorizedAppException("Kullanıcı hesabı pasif durumdadır.");
        }

        user.RefreshToken = jwtTokenGenerator.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);
        user.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await securityLogService.LogSecurityAuditAsync(
            user.Id,
            user.Email,
            "LoginSuccess",
            "Kullanıcı başarıyla giriş yaptı.",
            cancellationToken);

        var (accessToken, expiresAt) = jwtTokenGenerator.GenerateAccessToken(user);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            accessToken,
            user.RefreshToken,
            expiresAt);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (user is null || user.RefreshTokenExpiryTime <= DateTime.UtcNow || !user.IsActive)
        {
            await securityLogService.LogFailedLoginAsync(
                user?.Email ?? "Bilinmiyor (RefreshToken)",
                user?.Id,
                "Geçersiz veya süresi dolmuş Refresh Token kullanımı.",
                cancellationToken);

            throw new UnauthorizedAppException("Geçersiz veya süresi dolmuş refresh token.");
        }

        user.RefreshToken = jwtTokenGenerator.GenerateRefreshToken();
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(RefreshTokenExpiryDays);
        user.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await securityLogService.LogSecurityAuditAsync(
            user.Id,
            user.Email,
            "TokenRefreshed",
            "Refresh token kullanılarak yeni oturum anahtarı üretildi.",
            cancellationToken);

        var (accessToken, expiresAt) = jwtTokenGenerator.GenerateAccessToken(user);

        return new AuthResponseDto(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            accessToken,
            user.RefreshToken,
            expiresAt);
    }

    public async Task RevokeTokenAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(userId, cancellationToken)
            ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        user.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await securityLogService.LogSecurityAuditAsync(
            user.Id,
            user.Email,
            "TokenRevoked",
            "Kullanıcı oturumu (Refresh Token) sonlandırıldı.",
            cancellationToken);
    }
}
