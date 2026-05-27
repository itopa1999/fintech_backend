using System.ComponentModel.DataAnnotations.Schema;
using Backend.Domain.Common;

namespace Backend.Domain.Entities;

[Table("BankAccounts")]
public class BankAccount : BaseEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string BankName { get; set; } 
    public string AccountName { get; set; } 
    public string AccountNumber { get; set; } 
    public bool IsDefault { get; set; } = false;
    public User User { get; set;}
}