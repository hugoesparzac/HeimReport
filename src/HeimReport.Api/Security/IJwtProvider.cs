using HeimReport.Api.Entities;

namespace HeimReport.Api.Security;

public interface IJwtProvider
{
    string GenerateToken(User user);
}