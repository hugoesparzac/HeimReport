using HeimReport.Api.DTOs.Auth;
using HeimReport.Api.Email;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using HeimReport.Api.Exceptions;
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

    // ===================== REGISTRATION =====================

    [Fact]
    public async Task RegisterAsync_ShouldSucced_WhenEmployeeIsActiveAndHasNoAccount()
    {
        // Arrange
        var employee = CreateValidEmployee();
        var dto = CreateValidRegistrationDto();

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(dto.Email.ToUpperInvariant(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _userRepository
            .Setup(r => r.GetByEmployeeIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepository
            .Setup(r => r.GetByNormalizedUsernameAsync(dto.Username.ToUpperInvariant(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasher.Setup(h => h.Hash(dto.Password)).Returns("hashed-password");
        _tokenHasher.Setup(h => h.GenerateRawToken()).Returns("raw-token");
        _tokenHasher.Setup(h => h.Hash("raw-token")).Returns("hashed-token");

        // Act
        await _sut.RegisterAsync(dto);

        // Assert
        _userRepository.Verify(r => r.AddAsync(It.Is<User>(u => u.EmployeeId == employee.Id &&
            u.PasswordHash == "hashed-password" &&
            u.EmailVerificationTokenHash == "hashed-token"
        ), It.IsAny<CancellationToken>()), Times.Once);

        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailSender.Verify(e => e.SendEmailVerificationAsync(employee.Email, "raw-token", dto.PreferredLanguage, It.IsAny<CancellationToken>()),Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailDoesNotMatchAnyActiveEmployee()
    {
        // Arrange
        var dto = CreateValidRegistrationDto();

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.RegisterAsync(dto));
        Assert.Equal("Unable to complete registration with the provided information.", exception.Message);

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _emailSender.Verify(e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Language>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmployeeAlreadyHasAnAccount()
    {
        // Arrange
        var employee = CreateValidEmployee();
        var dto = CreateValidRegistrationDto();
        var existingUser = CreateExistingUser(employee.Id, "existing");

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _userRepository
            .Setup(r => r.GetByEmployeeIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.RegisterAsync(dto));
        Assert.Equal("Unable to complete registration with the provided information.", exception.Message);

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailSender.Verify(e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Language>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenUsernameIsAlreadyTaken()
    {
        // Arrange
        var employee = CreateValidEmployee();
        var dto = CreateValidRegistrationDto();
        var someoneElsesAccount = CreateExistingUser(999, dto.Username);

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _userRepository
            .Setup(r => r.GetByEmployeeIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepository
            .Setup(r => r.GetByNormalizedUsernameAsync(dto.Username.ToUpperInvariant(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(someoneElsesAccount);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.RegisterAsync(dto));
        Assert.Equal("This username is already taken. Please choose a different one.", exception.Message);

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailSender.Verify(e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Language>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== HELPER METHODS (FACTORIES) =====================

    private static Employee CreateValidEmployee() => new()
    {
        Id = 1,
        FirstName = "Hugo",
        LastName = "Esparza",
        Email = "hugo@heimreport.com",
        NormalizedEmail = "HUGO@HEIMREPORT.COM",
        NationalId = "ABC123",
        HireDate = DateTime.UtcNow.AddYears(-1),
        ContractType = ContractType.Permanent,
        Status = EmployeeStatus.Active,
        CountryId = 1,
        DepartmentId = 1,
        PositionId = 1,
        CreatedAt = DateTime.UtcNow
    };

    private static UserRegistrationDto CreateValidRegistrationDto() => new()
    {
        Email = "hugo@heimreport.com",
        Username = "hugoesparzac",
        Password = "SecurePass123!",
        ConfirmPassword = "SecurePass123!",
        PreferredLanguage = Language.English
    };

    private static User CreateExistingUser(int employeeId, string username) => new()
    {
        Id = 99,
        EmployeeId = employeeId,
        Username = username,
        NormalizedUsername = username.ToUpperInvariant(),
        PasswordHash = "already-hashed",
        Role = SystemRole.Employee,
        IsEmailVerified = true,
        IsActive = true,
        PreferredLanguage = Language.English,
        CreatedAt = DateTime.UtcNow
    };
}