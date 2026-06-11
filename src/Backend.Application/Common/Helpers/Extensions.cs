
using Backend.Domain.Entities;

namespace Backend.Application.Common.Helpers;

public static class Extensions
{
    public static bool IsTierOneComplete(this KycProfile kyc)
    {
        return !string.IsNullOrEmpty(kyc.BVN) ||
               !string.IsNullOrEmpty(kyc.NIN);
    }
}