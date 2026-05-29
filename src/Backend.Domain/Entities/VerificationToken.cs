using System.ComponentModel.DataAnnotations.Schema;
using Backend.Domain.Entities;

namespace Backend.Domain.Common;

[Table("VerificationTokens")]
public class VerificationToken : BaseEntity
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public int Token { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? UsedAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public User? User { get; set; } = null!;
}