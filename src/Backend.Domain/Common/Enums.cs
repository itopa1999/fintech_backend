namespace Backend.Domain.Common;

public enum AccountStatus
{
    Pending,
    Active,
    Suspended,
    Locked,
    Closed
}

public enum VerificationStatus
{
    NotSubmitted,
    Pending,
    InReview,
    Approved,
    Rejected
}

public enum UserRole
{
    Admin,
    Organizer,
    User
}

public enum VerificationTokenType
{
    EmailVerification,
    PasswordReset
}

public enum KycTier
{
    Tier0 = 0, // No KYC
    Tier1 = 1, // Basic KYC - BVN or NIN
    Tier2 = 2, // Enhanced KYC - ID + Selfie
    Tier3 = 3  // Full KYC - ID + Selfie + Proof of Address
}