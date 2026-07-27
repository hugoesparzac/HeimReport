using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using HeimReport.Api.Security;
using Microsoft.Extensions.Options;

namespace HeimReport.Api.UnitTests.Security;

public class JwtProviderTests
{
    private readonly JwtProvider _sut;
    private readonly JwtOptions _options;

    public JwtProviderTests()
    {
        _options = new JwtOptions
        {
            Key = "test-key-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        };

        _sut = new JwtProvider(Options.Create(_options));
    }

    private static User BuildUserWithEmployee(int userId = 1, SystemRole role = SystemRole.HR) => new()
    {
        Id = userId,
        EmployeeId = 10,
        Employee = new Employee
        {
            Id = 10,
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@heimreport-demo.com",
            NormalizedEmail = "JANE.DOE@HEIMREPORT-DEMO.COM",
            NationalId = "SEED-00001",
            BirthDate = DateTime.UtcNow.AddYears(-30),
            HireDate = DateTime.UtcNow,
            ContractType = ContractType.Permanent,
            CurrentSalary = 25000,
            CountryId = 1,
            DepartmentId = 1,
            PositionId = 1,
            CreatedAt = DateTime.UtcNow
        },
        Username = "jane.doe",
        NormalizedUsername = "JANE.DOE",
        PasswordHash = "hashed",
        Role = role,
        IsEmailVerified = true,
        IsActive = true,
        PreferredLanguage = Language.English,
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public void GenerateToken_ShouldThrow_WhenUserEmployeeIsNotLoaded()
    {
        // Arrange
        var user = BuildUserWithEmployee();
        user.Employee = null;

        // Act
        void Act() => _sut.GenerateToken(user);

        // Assert
        var exception = Assert.Throws<InvalidOperationException>(Act);
        Assert.Contains("Ensure the query includes .Include(u => u.Employee)", exception.Message);
    }

    [Fact]
    public void GenerateToken_ShouldIncludeExpectedClaims()
    {
        // Arrange
        var user = BuildUserWithEmployee(userId: 42, role: SystemRole.Admin);

        // Act
        var token = _sut.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal("42", jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal("10", jwt.Claims.First(c => c.Type == "employeeId").Value);
        Assert.Equal(user.Employee!.Email, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(user.Username, jwt.Claims.First(c => c.Type == JwtRegisteredClaimNames.Name).Value);
        Assert.Equal(nameof(SystemRole.Admin), jwt.Claims.First(c => c.Type == "role").Value);
    }

    [Fact]
    public void GenerateToken_ShouldSetIssuerAndAudience_FromOptions()
    {
        // Arrange
        var user = BuildUserWithEmployee();

        // Act
        var token = _sut.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        Assert.Equal(_options.Issuer, jwt.Issuer);
        Assert.Equal(_options.Audience, jwt.Audiences.First());
    }

    [Fact]
    public void GenerateToken_ShouldSetExpiration_AccordingToConfiguredMinutes()
    {
        // Arrange
        var user = BuildUserWithEmployee();
        var before = DateTime.UtcNow.AddMinutes(_options.ExpirationInMinutes);

        // Act
        var token = _sut.GenerateToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);
        var after = DateTime.UtcNow.AddMinutes(_options.ExpirationInMinutes);

        Assert.InRange(jwt.ValidTo, before.AddSeconds(-5), after.AddSeconds(5));
    }

    [Fact]
    public void GenerateToken_ShouldProduceDifferentTokens_ForDifferentUsersOnEachCall()
    {
        // Arrange
        var user = BuildUserWithEmployee();

        // Act
        var token1 = _sut.GenerateToken(user);
        var token2 = _sut.GenerateToken(user);

        // Assert
        Assert.NotEqual(token1, token2);
    }
}