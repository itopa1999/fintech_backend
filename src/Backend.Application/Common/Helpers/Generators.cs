using System.Security.Cryptography;
using System.Text;

namespace Backend.Application.Common.Helpers;

public static class Generators
{
    public static string Generate(int digits = 6)
    {
        if (digits < 4)
            throw new ArgumentException("OTP must be at least 4 digits.");

        var min = (int)Math.Pow(10, digits - 1);
        var max = (int)Math.Pow(10, digits);

        var otp = RandomNumberGenerator.GetInt32(min, max);

        return otp.ToString();
    }

    public static string GeneratePassword(int length = 10)
    {

        if (length < 8)
            throw new ArgumentException("Password must be at least 8 characters.");

        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";

        var password = new StringBuilder();

        password.Append(GetRandomChar(upper));
        password.Append(GetRandomChar(lower));
        password.Append(GetRandomChar(digits));

        string allChars = upper + lower + digits;

        for (int i = password.Length; i < length; i++)
        {
            password.Append(GetRandomChar(allChars));
        }

        return Shuffle(password.ToString());
    }

    private static char GetRandomChar(string chars)
    {
        return chars[RandomNumberGenerator.GetInt32(chars.Length)];
    }

    private static string Shuffle(string input)
    {
        var array = input.ToCharArray();

        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (array[i], array[j]) = (array[j], array[i]);
        }

        return new string(array);
    }
}