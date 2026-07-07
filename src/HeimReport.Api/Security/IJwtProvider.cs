using System.Security.Claims;
using HeimReport.Api.Entities;

namespace HeimReport.Api.Security;

public interface IJwtProvider
{
    string GenerateToken(Employee employee);
}