using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Application.DTOs.Kyc;
public class CreateKycTierOneDto
{
    [Required]
    [MinLength(11, ErrorMessage = "BVN must be exactly 11 characters")]
    public string BVN { get; set; }
    [Required]
    [MinLength(11, ErrorMessage = "NIN must be exactly 11 characters")]
    public string NIN { get; set; }
}

public class CreateKycTierTwoDto
{
    [Required]
    public IFormFile IdDocument { get; set; }
    
    [Required]
    public IFormFile Selfie { get; set; }
}

public class CreateKycTierThreeDto
{   
    [Required]
    public string AddressLine1 { get; set; } 

    public string? AddressLine2 { get; set; }

    [Required]
    public string City { get; set; } 
    [Required]

    public string State { get; set; } 

    [Required]
    public string Country { get; set; } 

    public string? PostalCode { get; set; }
}