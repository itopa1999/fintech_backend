using System.ComponentModel.DataAnnotations;

namespace Backend.Application.DTOs.Auth;

public class AuthResponseDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Role { get; set; }
    public int KycTier { get; set; }
    public string Status { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }

}

public class LoginUserDto
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; set; }
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }

}

public class VerifyTokenDto
{
    public int UserId { get; set; }
    public int Token { get; set; }
}

public class RefreshRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; }
}