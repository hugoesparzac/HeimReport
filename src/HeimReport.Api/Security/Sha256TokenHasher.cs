using System.Security.Cryptography;
using System.Text;

namespace HeimReport.Api.Security;

public sealed class Sha256TokenHasher : ITokenHasher
{
    public string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes);
    }

    public string GenerateRawToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToHexString(randomBytes);
    }
}