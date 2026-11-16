using System.ComponentModel.DataAnnotations;

namespace BusBookingApi.DTOs;

public sealed class RegisterRequest
{
    [Required(ErrorMessage = "Username is required."), StringLength(32, MinimumLength = 4, ErrorMessage = "Username must be between 4 and 32 characters."), RegularExpression(@"^\S+$", ErrorMessage = "Username cannot contain spaces.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required."), EmailAddress(ErrorMessage = "Enter a valid email address."), StringLength(254)]
    public string EmailId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Full name is required."), StringLength(100, MinimumLength = 3, ErrorMessage = "Full name must be at least 3 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required."), StringLength(72, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters."), RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$", ErrorMessage = "Password must include an uppercase letter, a lowercase letter, a number and a special character.")]
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    [Required, StringLength(32)]
    public string UserName { get; set; } = string.Empty;

    [Required, StringLength(72)]
    public string Password { get; set; } = string.Empty;
}

public sealed record AuthUserDto(int UserId, string UserName, string FullName);
public sealed record LoginResponse(string Token, int UserId, string UserName, string FullName);
