using System.ComponentModel.DataAnnotations;

namespace CleanAuthApi.Application.DTOs.Users;

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);

public record UpdateProfileRequestDto(
    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [MaxLength(100)]
    string FirstName,

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [MaxLength(100)]
    string LastName
);

public record ChangePasswordRequestDto(
    [Required(ErrorMessage = "Mevcut şifre zorunludur.")]
    string CurrentPassword,

    [Required(ErrorMessage = "Yeni şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Yeni şifre en az 6 karakter olmalıdır.")]
    string NewPassword
);
