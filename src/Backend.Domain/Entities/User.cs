using System.ComponentModel.DataAnnotations.Schema;
using Backend.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace Backend.Domain.Entities;

[Table("Users")]
public class User : IdentityUser<int>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public int KycTier { get; set; } = 0;
    public AccountStatus Status { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? PasswordChangedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    
    public KycProfile? KycProfile { get; set; }
    public ICollection<UserAddress> Addresses { get; set; }
        = new List<UserAddress>();
    public ICollection<Device> Devices { get; set; }
        = new List<Device>();
    public ICollection<BankAccount> BankAccounts { get; set; }
        = new List<BankAccount>();
    public string FullName => string.Join(" ", new[] { FirstName, LastName }
        .Where(value => !string.IsNullOrWhiteSpace(value)));
}
