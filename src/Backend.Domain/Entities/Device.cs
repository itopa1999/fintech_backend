using System.ComponentModel.DataAnnotations.Schema;
using Backend.Domain.Common;

namespace Backend.Domain.Entities;

[Table("Devices")]
public class Device : BaseEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string DeviceName { get; set; } 
    public string DeviceType { get; set; } 
    public string? IpAddress { get; set; }
    public bool IsTrusted { get; set; } = false;
    public DateTime? LastLoginAt { get; set; }
    public User User { get; set; } 
}