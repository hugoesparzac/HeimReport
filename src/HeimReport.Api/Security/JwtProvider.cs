using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HeimReport.Api.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
namespace HeimReport.Api.Security;

public sealed class JwtProvider(IOptions<JwtOptions> jwtOptions) : IJwtProvider
{
    private readonly JwtOptions _options = jwtOptions.Value;

    public string GenerateToken(User user)
    {
        if (user.Employee is null)
        {
            throw new InvalidOperationException(
                $"Cannot generate token: User with Id {user.Id} was loaded without its related Employee. " +
                "Ensure the query includes .Include(u => u.Employee).");
        }

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("employeeId", user.EmployeeId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Employee.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpirationInMinutes),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(securityToken);
    }
}