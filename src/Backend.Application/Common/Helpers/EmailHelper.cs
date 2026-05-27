using System.Text.RegularExpressions;

namespace Backend.Application.Common.Helpers;

public static class EmailHelper
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    /// <summary>
    /// Returns true only if email is valid
    /// </summary>
    public static bool IsValid(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        email = email.Trim();

        return EmailRegex.IsMatch(email);
    }
}