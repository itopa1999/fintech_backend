using Backend.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Backend.Domain.Entities;

public class User : IdentityUser<int>, IBaseEntity
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? PasswordChangedAt { get; set; }

    public string FullName => string.Join(" ", new[] { FirstName, LastName }
        .Where(value => !string.IsNullOrWhiteSpace(value)));
}
