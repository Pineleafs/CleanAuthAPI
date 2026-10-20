using CleanAuthApi.Application.Common.Exceptions;
using CleanAuthApi.Application.DTOs.Users;
using CleanAuthApi.Application.Interfaces;
using CleanAuthApi.Domain.Entities;
using CleanAuthApi.Domain.Interfaces;

namespace CleanAuthApi.Application.Services;

public class UserService(
    IUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ISecurityLogService securityLogService) : IUserService
{
    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        return MapToDto(user);
    }

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await unitOfWork.Users.GetAllAsync(cancellationToken);
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto> UpdateProfileAsync(Guid id, UpdateProfileRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await securityLogService.LogSecurityAuditAsync(
            user.Id,
            user.Email,
            "ProfileUpdated",
            "Kullanıcı profil bilgilerini güncelledi.",
            cancellationToken);

        return MapToDto(user);
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        if (!passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            await securityLogService.LogPasswordChangeAsync(
                user.Id,
                user.Email,
                isSuccess: false,
                failureReason: "Mevcut şifre hatalı girildi.",
                cancellationToken);

            throw new AppException("Mevcut şifreniz hatalı.", 400);
        }

        user.PasswordHash = passwordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        unitOfWork.Users.Update(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await securityLogService.LogPasswordChangeAsync(
            user.Id,
            user.Email,
            isSuccess: true,
            failureReason: null,
            cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Kullanıcı bulunamadı.");

        unitOfWork.Users.Remove(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await securityLogService.LogSecurityAuditAsync(
            user.Id,
            user.Email,
            "UserDeleted",
            "Kullanıcı hesabı silindi.",
            cancellationToken);
    }

    private static UserDto MapToDto(User user) =>
        new(
            user.Id,
            user.Email,
            user.FirstName,
            user.LastName,
            user.FullName,
            user.Role.ToString(),
            user.IsActive,
            user.CreatedAt);
}
