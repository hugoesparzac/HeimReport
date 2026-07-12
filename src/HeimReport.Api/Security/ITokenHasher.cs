namespace HeimReport.Api.Security;

public interface ITokenHasher
{
    string Hash(string rawToken);
    string GenerateRawToken();
}