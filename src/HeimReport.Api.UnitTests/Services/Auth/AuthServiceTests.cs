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
using static System.Runtime.InteropServices.JavaScript.JSType;

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
        var employee = new Employee
        {
            Id = 1,
            FirstName = "Hugo",
            LastName = "Esparza",
            Email = "hugo@heimreport.com",
            NormalizedEmail = "HUGO@HEIMREPORT.COM",
            NationalId = "ABC123",
            HireDate = DateTime.UtcNow.AddYears(-1),
            ContractType = ContractType.Permanent,
            CountryId = 1,
            DepartmentId = 1,
            PositionId = 1,
            CreatedAt = DateTime.UtcNow
        };

        var dto = new UserRegistrationDto
        {
            Email = "hugo@heimreport.com",
            Username = "hugoesparzac",
            Password = "SecurePass123!",
            ConfirmPassword = "SecurePass123!",
            PreferredLanguage = Language.English
        };

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _userRepository
            .Setup(r => r.GetByEmployeeIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _passwordHasher.Setup(h => h.Hash(dto.Password)).Returns("hashed-password");
        _tokenHasher.Setup(h => h.GenerateRawToken()).Returns("raw-token");
        _tokenHasher.Setup(h => h.Hash("raw-token")).Returns("hashed-token");

        // Act
        await _sut.RegisterAsync(dto);

        // Assert
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailSender.Verify(
            e => e.SendEmailVerificationAsync(employee.Email, "raw-token", Language.English, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailDoesNotMatchAnyActiveEmployee()
    {
        // Arrange
        var dto = new UserRegistrationDto
        {
            Email = "unknown@heimreport.com",
            Username = "someone",
            Password = "SecurePass123!",
            ConfirmPassword = "SecurePass123!",
            PreferredLanguage = Language.English
        };

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _sut.RegisterAsync(dto));
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmployeeAlreadyHasAnAccount()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            FirstName = "Hugo",
            LastName = "Esparza",
            Email = "hugo@heimreport.com",
            NormalizedEmail = "HUGO@HEIMREPORT.COM",
            NationalId = "ABC123",
            HireDate = DateTime.UtcNow.AddYears(-1),
            ContractType = ContractType.Permanent,
            CountryId = 1,
            DepartmentId = 1,
            PositionId = 1,
            CreatedAt = DateTime.UtcNow
        };

        var existingUser = new User
        {
            Id = 5,
            EmployeeId = employee.Id,
            Username = "existing",
            NormalizedUsername = "EXISTING",
            PasswordHash = "already-hashed",
            Role = SystemRole.Employee,
            IsEmailVerified = true,
            IsActive = true,
            PreferredLanguage = Language.English,
            CreatedAt = DateTime.UtcNow
        };

        var dto = new UserRegistrationDto
        {
            Email = "hugo@heimreport.com",
            Username = "newattempt",
            Password = "SecurePass123!",
            ConfirmPassword = "SecurePass123!",
            PreferredLanguage = Language.English
        };

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _userRepository
            .Setup(r => r.GetByEmployeeIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _sut.RegisterAsync(dto));
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenUsernameIsAlreadyTaken()
    {
        // Arrange
        var employee = new Employee
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

        var dto = new UserRegistrationDto
        {
            Email = "hugo@heimreport.com",
            Username = "takenusername",
            Password = "SecurePass123!",
            ConfirmPassword = "SecurePass123!",
            PreferredLanguage = Language.English
        };

        var someoneElsesAccount = new User
        {
            Id = 99,
            EmployeeId = 999,
            Username = "takenusername",
            NormalizedUsername = "TAKENUSERNAME",
            PasswordHash = "hashed",
            Role = SystemRole.Employee,
            IsEmailVerified = true,
            IsActive = true,
            PreferredLanguage = Language.English,
            CreatedAt = DateTime.UtcNow
        };

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _userRepository
            .Setup(r => r.GetByEmployeeIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _userRepository
            .Setup(r => r.GetByNormalizedUsernameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(someoneElsesAccount);

        // Act & Assert
        await Assert.ThrowsAsync<DomainException>(() => _sut.RegisterAsync(dto));
        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

}
