using System.ComponentModel.DataAnnotations;
using Backend.Domain.Common;

namespace Backend.Application.DTOs.Auth;

public class RegisterUserDto
{
    [Required]
    public string FirstName { get; set; }
    [Required]
    public string LastName { get; set; }
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; set; }
    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters long.")]
    [DataType(DataType.Password)]
    public string Password { get; set; }
    [Required]
    public string RoleType { get; set; } = UserRole.User.ToString();
}