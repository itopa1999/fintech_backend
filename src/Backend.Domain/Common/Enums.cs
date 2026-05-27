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