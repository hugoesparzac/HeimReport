using System.Reflection.Metadata;
using Bogus;
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
        var employee = GetEmployeeFaker().Generate();

        var dto = GetRegistrationDtoFaker(employee.Email).Generate();

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
        var normalizedUsername = dto.Username.ToUpperInvariant();

        _userRepository.Verify(r => r.AddAsync(
            It.Is<User>(u =>
                u.EmployeeId == employee.Id &&
                u.NormalizedUsername == normalizedUsername &&
                u.PasswordHash == "hashed-password" &&
                u.EmailVerificationTokenHash == "hashed-token"),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailSender.Verify(
            e => e.SendEmailVerificationAsync(employee.Email, "raw-token", dto.PreferredLanguage, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailDoesNotMatchAnyActiveEmployee()
    {
        // Arrange
        var dto = GetRegistrationDtoFaker().Generate();

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
        var employee = GetEmployeeFaker().Generate();
        var dto = GetRegistrationDtoFaker(employee.Email).Generate();
        var existingUser = GetUserFaker(employeeId: employee.Id).Generate();

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
        var employee = GetEmployeeFaker().Generate();
        var dto = GetRegistrationDtoFaker(employee.Email).Generate();
        var someoneElsesAccount = GetUserFaker(employeeId: 999, username: dto.Username).Generate();

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

    // ===================== EMAIL VERIFICATION =====================

    [Fact]
    public async Task VerifyEmailAsync_ShouldSucceed_WhenTokenIsValidAndNotExpired()
    {
        // Arrange
        const string rawToken = "raw-token";
        const string tokenHash = "hashed-token";

        var user = GetUserFaker(
            isEmailVerified: false,
            emailVerificationTokenHash: tokenHash,
            emailVerificationTokenExpiresAt: DateTime.UtcNow.AddHours(1)
        ).Generate();

        _tokenHasher
            .Setup(h => h.Hash(rawToken))
            .Returns(tokenHash);

        _userRepository
            .Setup(r => r.GetByEmailVerificationTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        await _sut.VerifyEmailAsync(rawToken);

        // Assert
        Assert.True(user.IsEmailVerified);
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_ShouldThrow_WhenTokenIsExpired()
    {
        // Arrange
        const string rawToken = "raw-token";
        const string tokenHash = "hashed-token";

        var user = GetUserFaker(
            isEmailVerified: false,
            emailVerificationTokenHash: tokenHash,
            emailVerificationTokenExpiresAt: DateTime.UtcNow.AddHours(-1)
        ).Generate();

        _tokenHasher
            .Setup(h => h.Hash(rawToken))
            .Returns(tokenHash);

        _userRepository
            .Setup(r => r.GetByEmailVerificationTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.VerifyEmailAsync(rawToken));
        Assert.Equal("This verification link has expired. Please request a new one.", exception.Message);

        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VerifyEmailAsync_ShouldThrow_WhenTokenIsNotFound()
    {
        // Arrange
        const string rawToken = "invalid-token";
        const string tokenHash = "hashed-token";

        _tokenHasher
            .Setup(h => h.Hash(rawToken))
            .Returns(tokenHash);

        _userRepository
            .Setup(r => r.GetByEmailVerificationTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.VerifyEmailAsync(rawToken));
        Assert.Equal("This verification link is invalid or has already been used.", exception.Message);

        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== LOGIN =====================

    [Fact]
    public async Task LoginAsync_ShouldSucceed_WhenCredentialsAreValidAndAccountIsVerifiedAndActive()
    {
        // Arrange
        var user = GetUserFaker().Generate();
        var dto = GetLoginDtoFaker(user.Username).Generate();

        _userRepository
            .Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify(dto.Password, user.PasswordHash))
            .Returns(true);

        _jwtProvider
            .Setup(j => j.GenerateToken(user))
            .Returns("access-token");

        _tokenHasher
            .Setup(h => h.GenerateRawToken())
            .Returns("raw-refresh-token");

        _tokenHasher
            .Setup(h => h.Hash("raw-refresh-token"))
            .Returns("hashed-refresh-token");

        // Act
        var result = await _sut.LoginAsync(dto);

        // Assert
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("raw-refresh-token", result.RefreshToken);

        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsWrong()
    {
        // Arrange
        var user = GetUserFaker().Generate();
        var dto = GetLoginDtoFaker(user.Username, "WrongPassword").Generate();

        _userRepository
            .Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify(dto.Password, user.PasswordHash))
            .Returns(false);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.LoginAsync(dto));
        Assert.Equal("Invalid username/email or password.", exception.Message);

        _jwtProvider.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserIsNotFound()
    {
        // Arrange
        var dto = GetLoginDtoFaker().Generate();

        _userRepository
            .Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.LoginAsync(dto));
        Assert.Equal("Invalid username/email or password.", exception.Message);

        _passwordHasher.Verify(h => h.Verify(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _jwtProvider.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenEmailIsNotVerified()
    {
        // Arrange
        var user = GetUserFaker(isEmailVerified: false).Generate();
        var dto = GetLoginDtoFaker(user.Username).Generate();

        _userRepository
            .Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify(dto.Password, user.PasswordHash))
            .Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.LoginAsync(dto));
        Assert.Equal("Please verify your email address before logging in.", exception.Message);

        _jwtProvider.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenAccountIsInactive()
    {
        // Arrange
        var user = GetUserFaker(isActive: false).Generate();
        var dto = GetLoginDtoFaker(user.Username).Generate();

        _userRepository
            .Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher
            .Setup(h => h.Verify(dto.Password, user.PasswordHash))
            .Returns(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.LoginAsync(dto));
        Assert.Equal("This account has been deactivated. Please contact support.", exception.Message);

        _jwtProvider.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== REFRESH =====================

    [Fact]
    public async Task RefreshAsync_ShouldSucceed_WhenTokenIsValidAndNotExpiredOrRevoked()
    {
        // Arrange
        const string oldRawToken = "old-raw-token";
        const string oldHash = "old-hash";
        const string newRawToken = "new-raw-token";
        const string newHash = "new-hash";
        const string newAccessToken = "new-access-token";

        var employee = GetEmployeeFaker().Generate();
        var user = GetUserFaker(employeeId: employee.Id).Generate();
        user.Employee = employee;

        var storedToken = GetRefreshTokenFaker(user: user, tokenHash: oldHash).Generate();

        _tokenHasher
            .Setup(h => h.Hash(oldRawToken))
            .Returns(oldHash);

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync(oldHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _refreshTokenRepository
            .Setup(r => r.Revoke(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => token.RevokedAt = DateTime.UtcNow);

        _jwtProvider
            .Setup(j => j.GenerateToken(user))
            .Returns(newAccessToken);

        _tokenHasher
            .Setup(h => h.GenerateRawToken())
            .Returns(newRawToken);

        _tokenHasher
            .Setup(h => h.Hash(newRawToken))
            .Returns(newHash);

        // Act
        var result = await _sut.RefreshAsync(oldRawToken);

        // Assert
        Assert.Equal(newAccessToken, result.AccessToken);
        Assert.Equal(newRawToken, result.RefreshToken);
        Assert.Equal(newHash, storedToken.ReplacedByTokenHash);
        Assert.NotNull(storedToken.RevokedAt);

        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_ShouldThrow_WhenTokenIsNotFound()
    {
        // Arrange
        const string rawToken = "unknown-raw-token";
        const string tokenHash = "unknown-hash";

        _tokenHasher
            .Setup(h => h.Hash(rawToken))
            .Returns(tokenHash);

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.RefreshAsync(rawToken));
        Assert.Equal("Invalid refresh token.", exception.Message);

        _jwtProvider.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRevokeAllActiveTokens_WhenAnAlreadyRevokedTokenIsReused()
    {
        // Arrange
        const string rawToken = "reused-raw-token";
        const string tokenHash = "reused-hash";

        var storedToken = GetRefreshTokenFaker(
            tokenHash: tokenHash,
            revokedAt: DateTime.UtcNow.AddHours(-1)
        ).Generate();

        var otherActiveToken = GetRefreshTokenFaker(tokenHash: "another-active-hash").Generate();
        otherActiveToken.UserId = storedToken.UserId;

        _tokenHasher
            .Setup(h => h.Hash(rawToken))
            .Returns(tokenHash);

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _refreshTokenRepository
            .Setup(r => r.GetActiveByUserIdAsync(storedToken.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([otherActiveToken]);

        _refreshTokenRepository
            .Setup(r => r.Revoke(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => token.RevokedAt = DateTime.UtcNow);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.RefreshAsync(rawToken));
        Assert.Equal("This session is no longer valid. Please log in again.", exception.Message);

        Assert.NotNull(otherActiveToken.RevokedAt);

        _jwtProvider.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_ShouldThrow_WhenTokenIsExpired()
    {
        // Arrange
        const string rawToken = "expired-raw-token";
        const string tokenHash = "expired-hash";

        var storedToken = GetRefreshTokenFaker(
            tokenHash: tokenHash,
            expiresAt: DateTime.UtcNow.AddDays(-1)
        ).Generate();

        _tokenHasher
            .Setup(h => h.Hash(rawToken))
            .Returns(tokenHash);

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.RefreshAsync(rawToken));
        Assert.Equal("This session has expired. Please log in again.", exception.Message);

        _jwtProvider.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== LOGOUT =====================

    [Fact]
    public async Task LogoutAsync_ShouldRevokeToken_WhenTokenIsValidAndNotRevoked()
    {
        // Arrange
        const string rawToken = "raw-token";
        const string tokenHash = "valid-hash";

        var storedToken = GetRefreshTokenFaker(tokenHash: tokenHash).Generate();

        _tokenHasher
            .Setup(h => h.Hash(rawToken))
            .Returns(tokenHash);

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _refreshTokenRepository
            .Setup(r => r.Revoke(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => token.RevokedAt = DateTime.UtcNow);

        // Act
        await _sut.LogoutAsync(rawToken);

        // Assert
        Assert.NotNull(storedToken.RevokedAt);

        _refreshTokenRepository.Verify(r => r.Revoke(It.IsAny<RefreshToken>()), Times.Once);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ShouldDoNothing_WhenTokenIsNotFound()
    {
        // Arrange
        const string rawToken = "unknown-raw-token";
        const string tokenHash = "unknown-hash";

        _tokenHasher
            .Setup(h => h.Hash(rawToken))
            .Returns(tokenHash);

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        await _sut.LogoutAsync(rawToken);

        // Assert
        _refreshTokenRepository.Verify(r => r.Revoke(It.IsAny<RefreshToken>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_ShouldDoNothing_WhenTokenIsAlreadyRevoked()
    {
        // Arrange
        const string rawToken = "raw-token";
        const string tokenHash = "already-revoked-hash";

        var storedToken = GetRefreshTokenFaker(
            tokenHash: tokenHash,
            revokedAt: DateTime.UtcNow.AddHours(-1)
        ).Generate();

        _tokenHasher
            .Setup(h => h.Hash(rawToken))
            .Returns(tokenHash);

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        // Act
        await _sut.LogoutAsync(rawToken);

        // Assert
        _refreshTokenRepository.Verify(r => r.Revoke(It.IsAny<RefreshToken>()), Times.Never);
        _refreshTokenRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== RESEND VERIFICATION =====================

    [Fact]
    public async Task ResendVerificationAsync_ShouldSendNewToken_WhenUserExistsAndEmailNotVerified()
    {
        // Arrange
        const string newRawToken = "new-raw-token";
        const string newHashedToken = "new-hashed-token";

        var employee = GetEmployeeFaker().Generate();
        var user = GetUserFaker(employeeId: employee.Id, isEmailVerified: false).Generate();
        var dto = new ResendEmailVerificationDto { Email = employee.Email };

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(employee.NormalizedEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _userRepository
            .Setup(r => r.GetByEmployeeIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenHasher
            .Setup(h => h.GenerateRawToken())
            .Returns(newRawToken);

        _tokenHasher
            .Setup(h => h.Hash(newRawToken))
            .Returns(newHashedToken);

        // Act
        await _sut.ResendVerificationAsync(dto);

        // Assert
        Assert.Equal(newHashedToken, user.EmailVerificationTokenHash);

        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _emailSender.Verify(
            e => e.SendEmailVerificationAsync(employee.Email, newRawToken, user.PreferredLanguage, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResendVerificationAsync_ShouldDoNothing_WhenEmployeeDoesNotMatchAnyActiveEmployee()
    {
        // Arrange
        var dto = new ResendEmailVerificationDto { Email = "unknown@heimreport.com" };

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act
        await _sut.ResendVerificationAsync(dto);

        // Assert
        _userRepository.Verify(r => r.GetByEmployeeIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _emailSender.Verify(
            e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Language>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResendVerificationAsync_ShouldDoNothing_WhenEmployeeHasNoAccount()
    {
        // Arrange
        var employee = GetEmployeeFaker().Generate();
        var dto = new ResendEmailVerificationDto { Email = employee.Email };

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(employee.NormalizedEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _userRepository
            .Setup(r => r.GetByEmployeeIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        await _sut.ResendVerificationAsync(dto);

        // Assert
        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _emailSender.Verify(
            e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Language>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ===================== BOGUS FAKERS =====================

    private static Faker<Employee> GetEmployeeFaker() => new Faker<Employee>()
        .RuleFor(e => e.Id, f => f.IndexFaker + 1)
        .RuleFor(e => e.FirstName, f => f.Name.FirstName())
        .RuleFor(e => e.LastName, f => f.Name.LastName())
        .RuleFor(e => e.Email, (f, e) => f.Internet.Email(e.FirstName, e.LastName))
        .RuleFor(e => e.NormalizedEmail, (_, e) => e.Email.ToUpperInvariant())
        .RuleFor(e => e.NationalId, f => f.Random.Replace("###-###-###"))
        .RuleFor(e => e.HireDate, f => f.Date.Past(5))
        .RuleFor(e => e.ContractType, f => f.PickRandom<ContractType>())
        .RuleFor(e => e.Status, EmployeeStatus.Active)
        .RuleFor(e => e.CountryId, f => f.Random.Int(1, 10))
        .RuleFor(e => e.DepartmentId, f => f.Random.Int(1, 20))
        .RuleFor(e => e.PositionId, f => f.Random.Int(1, 50))
        .RuleFor(e => e.CreatedAt, f => f.Date.Past(6));

    private static Faker<UserRegistrationDto> GetRegistrationDtoFaker(string? email = null) => new Faker<UserRegistrationDto>()
        .RuleFor(d => d.Email, f => email ?? f.Internet.Email())
        .RuleFor(d => d.Username, f => f.Internet.UserName())
        .RuleFor(d => d.Password, _ => "P@ssw0rd123!")
        .RuleFor(d => d.ConfirmPassword, (_, d) => d.Password)
        .RuleFor(d => d.PreferredLanguage, f => f.PickRandom<Language>());

    private static Faker<User> GetUserFaker(
        int? employeeId = null,
        string? username = null,
        bool isEmailVerified = true,
        bool isActive = true,
        string? emailVerificationTokenHash = null,
        DateTime? emailVerificationTokenExpiresAt = null) => new Faker<User>()
        .RuleFor(u => u.Id, f => f.IndexFaker + 1)
        .RuleFor(u => u.EmployeeId, f => employeeId ?? f.Random.Int(100, 1000))
        .RuleFor(u => u.Username, f => username ?? f.Internet.UserName())
        .RuleFor(u => u.NormalizedUsername, (_, u) => u.Username.ToUpperInvariant())
        .RuleFor(u => u.PasswordHash, f => f.Internet.Password())
        .RuleFor(u => u.Role, f => f.PickRandom<SystemRole>())
        .RuleFor(u => u.IsEmailVerified, isEmailVerified)
        .RuleFor(u => u.EmailVerificationTokenHash, emailVerificationTokenHash)
        .RuleFor(u => u.EmailVerificationTokenExpiresAt, emailVerificationTokenExpiresAt)
        .RuleFor(u => u.IsActive, isActive)
        .RuleFor(u => u.PreferredLanguage, f => f.PickRandom<Language>())
        .RuleFor(u => u.CreatedAt, f => f.Date.Recent());

    private static Faker<UserLoginDto> GetLoginDtoFaker(string? usernameOrEmail = null, string? password = null) =>
    new Faker<UserLoginDto>()
        .RuleFor(d => d.UsernameOrEmail, f => usernameOrEmail ?? f.Internet.UserName())
        .RuleFor(d => d.Password, _ => password ?? "P@ssw0rd123!");

    private static Faker<RefreshToken> GetRefreshTokenFaker(
        User? user = null,
        string? tokenHash = null,
        DateTime? expiresAt = null,
        DateTime? revokedAt = null) => new Faker<RefreshToken>()
        .RuleFor(rt => rt.Id, f => f.IndexFaker + 1)
        .RuleFor(rt => rt.UserId, f => user?.Id ?? f.Random.Int(1, 1000))
        .RuleFor(rt => rt.User, user)
        .RuleFor(rt => rt.TokenHash, f => tokenHash ?? f.Random.Hash())
        .RuleFor(rt => rt.ExpiresAt, f => expiresAt ?? f.Date.Future())
        .RuleFor(rt => rt.CreatedAt, f => f.Date.Recent())
        .RuleFor(rt => rt.RevokedAt, revokedAt);
}