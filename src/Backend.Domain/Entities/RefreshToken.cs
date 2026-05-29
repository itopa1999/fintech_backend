using System.ComponentModel.DataAnnotations.Schema;
using Backend.Domain.Common;

namespace Backend.Domain.Entities;

[Table("RefreshTokens")]

public class RefreshToken : BaseEntity
{
    public int Id { get; set; }
    public string Token { get; set; } = default!;
    public int UserId { get; set; }
    public User User { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsActive => RevokedAt == null && DateTime.UtcNow < ExpiresAt;
}