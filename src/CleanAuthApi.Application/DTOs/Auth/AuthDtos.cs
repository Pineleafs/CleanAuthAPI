using System.ComponentModel.DataAnnotations;

namespace CleanAuthApi.Application.DTOs.Auth;

public record RegisterRequestDto(
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    string Email,

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
    string Password,

    [Required(ErrorMessage = "Ad alanı zorunludur.")]
    [MaxLength(100)]
    string FirstName,

    [Required(ErrorMessage = "Soyad alanı zorunludur.")]
    [MaxLength(100)]
    string LastName
);

public record LoginRequestDto(
    [Required(ErrorMessage = "E-posta zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    string Email,

    [Required(ErrorMessage = "Şifre zorunludur.")]
    string Password
);

public record RefreshTokenRequestDto(
    [Required(ErrorMessage = "Refresh token zorunludur.")]
    string RefreshToken
);

public record AuthResponseDto(
    Guid UserId,
    string Email,
    string FullName,
    string Role,
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt
);
