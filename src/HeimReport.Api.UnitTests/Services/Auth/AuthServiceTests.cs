using HeimReport.Api.Email;
using HeimReport.Api.Repositories.Auth;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Security;
using HeimReport.Api.Services.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace HeimReport.Api.UnitTests.Services.Auth;

public class AuthServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenHasher> _tokenHasher = new();
    private readonly Mock<IJwtProvider> _jwtProvider = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<ILogger<AuthService>> _logger = new();

    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var jwtOptions = Options.Create(new JwtOptions
        {
            Key = "test-key-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        });

        _sut = new AuthService(
            _employeeRepository.Object,
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _passwordHasher.Object,
            _tokenHasher.Object,
            _jwtProvider.Object,
            _emailSender.Object,
            jwtOptions,
            _logger.Object
        );
    }
}
