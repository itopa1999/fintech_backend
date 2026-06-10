
using Backend.Domain.Entities;

namespace Backend.Application.Common.Helpers;

public static class Extensions
{
    public static bool KycOneRequirements(this KycProfile kyc)
    {
        return !string.IsNullOrEmpty(kyc) &&
               !string.IsNullOrEmpty(kyc.Address) &&
               !string.IsNullOrEmpty(kyc.PhoneNumber) &&
               !string.IsNullOrEmpty(kyc.IdNumber);
    }
}