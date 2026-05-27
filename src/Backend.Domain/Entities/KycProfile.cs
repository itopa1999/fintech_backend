using System.ComponentModel.DataAnnotations.Schema;
using Backend.Domain.Common;

namespace Backend.Domain.Entities;

[Table("KycProfiles")]
public class KycProfile : BaseEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? BVN { get; set; }
    public string? NIN { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
    public string? VerificationProvider { get; set; }
    public string? SelfieUrl { get; set; }
    public string? IdDocumentUrl { get; set; }
    public decimal RiskScore { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public User User { get; set; } 
}