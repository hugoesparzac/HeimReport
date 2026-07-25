using System.Security.Cryptography;

namespace HeimReport.Api.Security;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const string UpperChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string LowerChars = "abcdefghijkmnpqrstuvwxyz";
    private const string DigitChars = "23456789";
    private const string SymbolChars = "!@#$%^&*";
    private const string AllChars = UpperChars + LowerChars + DigitChars + SymbolChars;

    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    public string GenerateTemporaryPassword()
    {
        const int length = 12;
        var chars = new char[length];

        chars[0] = PickRandomChar(UpperChars);
        chars[1] = PickRandomChar(LowerChars);
        chars[2] = PickRandomChar(DigitChars);
        chars[3] = PickRandomChar(SymbolChars);

        for (var i = 4; i < length; i++)
        {
            chars[i] = PickRandomChar(AllChars);
        }

        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    private static char PickRandomChar(string source)
    {
        return source[RandomNumberGenerator.GetInt32(source.Length)];
    }
}