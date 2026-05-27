using System.ComponentModel.DataAnnotations.Schema;
using Backend.Domain.Common;

namespace Backend.Domain.Entities;

[Table("UserAddresses")]
public class UserAddress : BaseEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string AddressLine1 { get; set; } 

    public string? AddressLine2 { get; set; }

    public string City { get; set; } 

    public string State { get; set; } 

    public string Country { get; set; } 

    public string? PostalCode { get; set; }

    public User User { get; set; } 
}