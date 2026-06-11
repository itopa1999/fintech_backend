
namespace Backend.Infrastructure.Helpers;


public static class RoleConstants
{
    public const string Admin = "Admin";
    public const string Organizer = "Organizer";
    public const string User = "User";
    public static readonly string[] AllRoles = new[] { Admin, Organizer, User };
}


public static class AcceptedFiles
{
    public const string jpeg = "jpeg";
    public const string png = "png";
}