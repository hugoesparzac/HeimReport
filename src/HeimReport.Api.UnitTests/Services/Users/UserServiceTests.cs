using Bogus;
using HeimReport.Api.DTOs.Users;
using HeimReport.Api.Email;
using HeimReport.Api.Entities;
using HeimReport.Api.Enums;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Repositories.Users;
using HeimReport.Api.Security;
using HeimReport.Api.Services.AuditLogs;
using HeimReport.Api.Services.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable;
using Moq;

namespace HeimReport.Api.UnitTests.Services.Users;

public class UserServiceTests
{
    private readonly Mock<IEmployeeRepository> _employeeRepository = new();
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenHasher> _tokenHasher = new();
    private readonly Mock<IJwtProvider> _jwtProvider = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<IAuditLogService> _auditLogService = new();
    private readonly Mock<ILogger<UserService>> _logger = new();

    private readonly UserService _sut;

    public UserServiceTests()
    {
        var jwtOptions = Options.Create(new JwtOptions
        {
            Key = "test-key-at-least-32-characters-long",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpirationInMinutes = 60,
            RefreshTokenExpirationInDays = 7
        });

        _auditLogService
            .Setup(a => a.LogAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int?>(),
                It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sut = new UserService(
            _employeeRepository.Object,
            _userRepository.Object,
            _refreshTokenRepository.Object,
            _passwordHasher.Object,
            _tokenHasher.Object,
            _jwtProvider.Object,
            _emailSender.Object,
            _auditLogService.Object,
            jwtOptions,
            _logger.Object
        );
    }

    // ===================== REGISTRATION =====================

    [Fact]
    public async Task RegisterAsync_ShouldSucceed_WhenEmployeeIsActiveAndHasNoAccount()
    {
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

        var createdUser = GetUserFaker(employeeId: employee.Id, username: dto.Username).Generate();
        createdUser.Employee = employee;
        createdUser.PreferredLanguage = dto.PreferredLanguage;
        createdUser.IsEmailVerified = false;

        _userRepository
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        var result = await _sut.RegisterAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.PreferredLanguage, result.PreferredLanguage);
        Assert.False(result.IsEmailVerified);

        _userRepository.Verify(r => r.AddAsync(
            It.Is<User>(u =>
                u.EmployeeId == employee.Id &&
                u.PasswordHash == "hashed-password" &&
                u.Role == SystemRole.Employee &&
                u.EmailVerificationTokenHash == "hashed-token"),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _emailSender.Verify(
            e => e.SendEmailVerificationAsync(employee.Email, "raw-token", dto.PreferredLanguage, It.IsAny<CancellationToken>()),
            Times.Once);

        _auditLogService.Verify(a => a.LogAsync(
            "REGISTER_USER", nameof(User), It.IsAny<int?>(),
            null, It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmailDoesNotMatchAnyActiveEmployee()
    {
        var dto = GetRegistrationDtoFaker().Generate();

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.RegisterAsync(dto));
        Assert.Equal("Unable to complete registration with the provided information.", exception.Message);

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenEmployeeAlreadyHasAnAccount()
    {
        var employee = GetEmployeeFaker().Generate();
        var dto = GetRegistrationDtoFaker(employee.Email).Generate();
        var existingUser = GetUserFaker(employeeId: employee.Id).Generate();

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _userRepository
            .Setup(r => r.GetByEmployeeIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.RegisterAsync(dto));
        Assert.Equal("Unable to complete registration with the provided information.", exception.Message);

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrow_WhenUsernameIsAlreadyTaken()
    {
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

        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.RegisterAsync(dto));
        Assert.Equal("This username is already taken. Please choose a different one.", exception.Message);

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== PROVISIONING =====================

    [Fact]
    public async Task ProvisionAsync_ShouldSucceed_WhenEmployeeExists()
    {
        var employee = GetEmployeeFaker().Generate();
        var dto = GetProvisionDtoFaker(employee.Id).Generate();

        _employeeRepository
            .Setup(r => r.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _passwordHasher.Setup(h => h.GenerateTemporaryPassword()).Returns("Temp@1234");
        _passwordHasher.Setup(h => h.Hash("Temp@1234")).Returns("hashed-temp-password");
        _tokenHasher.Setup(h => h.GenerateRawToken()).Returns("raw-token");
        _tokenHasher.Setup(h => h.Hash("raw-token")).Returns("hashed-token");

        var createdUser = GetUserFaker(employeeId: employee.Id, username: dto.Username).Generate();
        createdUser.Employee = employee;
        createdUser.Role = dto.Role;

        _userRepository
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdUser);

        var result = await _sut.ProvisionAsync(dto);

        Assert.NotNull(result);
        Assert.Equal(dto.Role, result.Role);

        _userRepository.Verify(r => r.AddAsync(
            It.Is<User>(u => u.EmployeeId == employee.Id && u.Role == dto.Role && u.PasswordHash == "hashed-temp-password"),
            It.IsAny<CancellationToken>()),
            Times.Once);

        _emailSender.Verify(
            e => e.SendTemporaryPasswordAsync(employee.Email, "Temp@1234", dto.PreferredLanguage, It.IsAny<CancellationToken>()),
            Times.Once);

        _emailSender.Verify(
            e => e.SendEmailVerificationAsync(employee.Email, "raw-token", dto.PreferredLanguage, It.IsAny<CancellationToken>()),
            Times.Once);

        _auditLogService.Verify(a => a.LogAsync(
            "PROVISION_USER", nameof(User), It.IsAny<int?>(),
            null, It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProvisionAsync_ShouldThrow_WhenEmployeeDoesNotExist()
    {
        var dto = GetProvisionDtoFaker(employeeId: 999).Generate();

        _employeeRepository
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.ProvisionAsync(dto));

        _userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== READ =====================

    [Fact]
    public async Task GetPagedAsync_ShouldReturnMappedPagedResult()
    {
        var employee1 = GetEmployeeFaker().Generate();
        var employee2 = GetEmployeeFaker().Generate();

        var user1 = GetUserFaker(employeeId: employee1.Id).Generate();
        user1.Employee = employee1;
        var user2 = GetUserFaker(employeeId: employee2.Id).Generate();
        user2.Employee = employee2;

        var users = new List<User> { user1, user2 };

        _userRepository
            .Setup(r => r.QueryWithDetails())
            .Returns(users.BuildMock());

        var query = new UserQueryDto { PageNumber = 1, PageSize = 10 };

        var result = await _sut.GetPagedAsync(query);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnUser_WhenFound()
    {
        var employee = GetEmployeeFaker().Generate();
        var user = GetUserFaker(employeeId: employee.Id).Generate();
        user.Employee = employee;

        _userRepository
            .Setup(r => r.GetByIdWithDetailsAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(user.Id);

        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrow_WhenNotFound()
    {
        _userRepository
            .Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(1));
    }

    // ===================== UPDATE =====================

    [Fact]
    public async Task UpdateAsync_ShouldUpdateFields_WithoutRevokingTokens_WhenStayingActive()
    {
        var user = GetUserFaker(isActive: true).Generate();
        var dto = new UserUpdateDto { Role = SystemRole.HR, IsActive = true, PreferredLanguage = Language.Spanish };

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _sut.UpdateAsync(user.Id, dto);

        Assert.Equal(SystemRole.HR, user.Role);
        Assert.Equal(Language.Spanish, user.PreferredLanguage);

        _refreshTokenRepository.Verify(
            r => r.RevokeAllActiveByUserIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);

        _auditLogService.Verify(a => a.LogAsync(
            "UPDATE_USER", nameof(User), user.Id,
            It.IsAny<object>(), It.IsAny<object>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldRevokeAllActiveTokens_WhenDeactivatingUser()
    {
        var user = GetUserFaker(isActive: true).Generate();
        var dto = new UserUpdateDto { Role = user.Role, IsActive = false, PreferredLanguage = user.PreferredLanguage };

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _sut.UpdateAsync(user.Id, dto);

        Assert.False(user.IsActive);

        _refreshTokenRepository.Verify(
            r => r.RevokeAllActiveByUserIdAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenUserNotFound()
    {
        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var dto = new UserUpdateDto { Role = SystemRole.Employee, IsActive = true, PreferredLanguage = Language.English };

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.UpdateAsync(1, dto));
    }

    // ===================== CHANGE PASSWORD =====================

    [Fact]
    public async Task ChangePasswordAsync_ShouldSucceed_WhenCurrentPasswordIsCorrect()
    {
        var user = GetUserFaker().Generate();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "OldP@ss1",
            NewPassword = "NewP@ss1",
            ConfirmNewPassword = "NewP@ss1"
        };

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher.Setup(h => h.Verify(dto.CurrentPassword, user.PasswordHash)).Returns(true);
        _passwordHasher.Setup(h => h.Hash(dto.NewPassword)).Returns("new-hashed-password");

        await _sut.ChangePasswordAsync(user.Id, dto);

        Assert.Equal("new-hashed-password", user.PasswordHash);

        _auditLogService.Verify(a => a.LogAsync(
            "CHANGE_PASSWORD", nameof(User), user.Id,
            null, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangePasswordAsync_ShouldThrow_WhenCurrentPasswordIsIncorrect()
    {
        var user = GetUserFaker().Generate();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "WrongPassword",
            NewPassword = "NewP@ss1",
            ConfirmNewPassword = "NewP@ss1"
        };

        _userRepository
            .Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher.Setup(h => h.Verify(dto.CurrentPassword, user.PasswordHash)).Returns(false);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.ChangePasswordAsync(user.Id, dto));
        Assert.Equal("Current password is incorrect.", exception.Message);

        _userRepository.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // ===================== EMAIL VERIFICATION =====================

    [Fact]
    public async Task VerifyEmailAsync_ShouldSucceed_WhenTokenIsValidAndNotExpired()
    {
        const string tokenHash = "hashed-token";
        var dto = new VerifyEmailDto { Token = "raw-token" };

        var user = GetUserFaker(
            isEmailVerified: false,
            emailVerificationTokenHash: tokenHash,
            emailVerificationTokenExpiresAt: DateTime.UtcNow.AddHours(1)
        ).Generate();

        _tokenHasher.Setup(h => h.Hash(dto.Token)).Returns(tokenHash);

        _userRepository
            .Setup(r => r.GetByEmailVerificationTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _sut.VerifyEmailAsync(dto);

        Assert.True(user.IsEmailVerified);
        Assert.Null(user.EmailVerificationTokenHash);
    }

    [Fact]
    public async Task VerifyEmailAsync_ShouldThrow_WhenTokenIsExpired()
    {
        const string tokenHash = "hashed-token";
        var dto = new VerifyEmailDto { Token = "raw-token" };

        var user = GetUserFaker(
            isEmailVerified: false,
            emailVerificationTokenHash: tokenHash,
            emailVerificationTokenExpiresAt: DateTime.UtcNow.AddHours(-1)
        ).Generate();

        _tokenHasher.Setup(h => h.Hash(dto.Token)).Returns(tokenHash);

        _userRepository
            .Setup(r => r.GetByEmailVerificationTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.VerifyEmailAsync(dto));
        Assert.Equal("This verification link has expired. Please request a new one.", exception.Message);
    }

    [Fact]
    public async Task VerifyEmailAsync_ShouldThrow_WhenTokenIsNotFound()
    {
        var dto = new VerifyEmailDto { Token = "invalid-token" };

        _tokenHasher.Setup(h => h.Hash(dto.Token)).Returns("hashed-token");

        _userRepository
            .Setup(r => r.GetByEmailVerificationTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.VerifyEmailAsync(dto));
        Assert.Equal("This verification link is invalid or has already been used.", exception.Message);
    }

    [Fact]
    public async Task ResendVerificationAsync_ShouldSendNewToken_WhenUserExistsAndEmailNotVerified()
    {
        var employee = GetEmployeeFaker().Generate();
        var user = GetUserFaker(employeeId: employee.Id, isEmailVerified: false).Generate();
        var dto = new ResendEmailVerificationDto { Email = employee.Email };

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(employee.NormalizedEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _userRepository
            .Setup(r => r.GetByEmployeeIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _tokenHasher.Setup(h => h.GenerateRawToken()).Returns("new-raw-token");
        _tokenHasher.Setup(h => h.Hash("new-raw-token")).Returns("new-hashed-token");

        await _sut.ResendVerificationAsync(dto);

        Assert.Equal("new-hashed-token", user.EmailVerificationTokenHash);

        _emailSender.Verify(
            e => e.SendEmailVerificationAsync(employee.Email, "new-raw-token", user.PreferredLanguage, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ResendVerificationAsync_ShouldDoNothing_WhenEmployeeDoesNotMatchAnyActiveEmployee()
    {
        var dto = new ResendEmailVerificationDto { Email = "unknown@heimreport.com" };

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        await _sut.ResendVerificationAsync(dto);

        _emailSender.Verify(
            e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Language>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ResendVerificationAsync_ShouldDoNothing_WhenEmailIsAlreadyVerified()
    {
        var employee = GetEmployeeFaker().Generate();
        var user = GetUserFaker(employeeId: employee.Id, isEmailVerified: true).Generate();
        var dto = new ResendEmailVerificationDto { Email = employee.Email };

        _employeeRepository
            .Setup(r => r.GetActiveByNormalizedEmailAsync(employee.NormalizedEmail, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _userRepository
            .Setup(r => r.GetByEmployeeIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _sut.ResendVerificationAsync(dto);

        _emailSender.Verify(
            e => e.SendEmailVerificationAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Language>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ===================== LOGIN =====================

    [Fact]
    public async Task LoginAsync_ShouldSucceed_WhenCredentialsAreValidAndAccountIsVerifiedAndActive()
    {
        var user = GetUserFaker().Generate();
        var dto = GetLoginDtoFaker(user.Username).Generate();

        _userRepository
            .Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);
        _jwtProvider.Setup(j => j.GenerateToken(user)).Returns("access-token");
        _tokenHasher.Setup(h => h.GenerateRawToken()).Returns("raw-refresh-token");
        _tokenHasher.Setup(h => h.Hash("raw-refresh-token")).Returns("hashed-refresh-token");

        var result = await _sut.LoginAsync(dto);

        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("raw-refresh-token", result.RefreshToken);
        Assert.NotNull(user.LastLoginAt);

        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenPasswordIsWrong()
    {
        var user = GetUserFaker().Generate();
        var dto = GetLoginDtoFaker(user.Username, "WrongPassword").Generate();

        _userRepository
            .Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(false);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.LoginAsync(dto));
        Assert.Equal("Invalid username/email or password.", exception.Message);

        _jwtProvider.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenUserIsNotFound()
    {
        var dto = GetLoginDtoFaker().Generate();

        _userRepository
            .Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.LoginAsync(dto));
        Assert.Equal("Invalid username/email or password.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenEmailIsNotVerified()
    {
        var user = GetUserFaker(isEmailVerified: false).Generate();
        var dto = GetLoginDtoFaker(user.Username).Generate();

        _userRepository
            .Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.LoginAsync(dto));
        Assert.Equal("Please verify your email address before logging in.", exception.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrow_WhenAccountIsInactive()
    {
        var user = GetUserFaker(isActive: false).Generate();
        var dto = GetLoginDtoFaker(user.Username).Generate();

        _userRepository
            .Setup(r => r.GetByUsernameOrEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHasher.Setup(h => h.Verify(dto.Password, user.PasswordHash)).Returns(true);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.LoginAsync(dto));
        Assert.Equal("This account has been deactivated. Please contact support.", exception.Message);
    }

    // ===================== REFRESH =====================

    [Fact]
    public async Task RefreshAsync_ShouldSucceed_WhenTokenIsValidAndNotExpiredOrRevoked()
    {
        const string oldHash = "old-hash";
        const string newRawToken = "new-raw-token";
        const string newHash = "new-hash";

        var employee = GetEmployeeFaker().Generate();
        var user = GetUserFaker(employeeId: employee.Id).Generate();
        user.Employee = employee;

        var storedToken = GetRefreshTokenFaker(user: user, tokenHash: oldHash).Generate();
        var dto = new RefreshTokenRequestDto { RefreshToken = "old-raw-token" };

        _tokenHasher.Setup(h => h.Hash("old-raw-token")).Returns(oldHash);

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync(oldHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _refreshTokenRepository
            .Setup(r => r.Revoke(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => token.RevokedAt = DateTime.UtcNow);

        _jwtProvider.Setup(j => j.GenerateToken(user)).Returns("new-access-token");
        _tokenHasher.Setup(h => h.GenerateRawToken()).Returns(newRawToken);
        _tokenHasher.Setup(h => h.Hash(newRawToken)).Returns(newHash);

        var result = await _sut.RefreshAsync(dto);

        Assert.Equal("new-access-token", result.AccessToken);
        Assert.Equal(newRawToken, result.RefreshToken);
        Assert.Equal(newHash, storedToken.ReplacedByTokenHash);
        Assert.NotNull(storedToken.RevokedAt);

        _refreshTokenRepository.Verify(r => r.AddAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RefreshAsync_ShouldThrow_WhenTokenIsNotFound()
    {
        var dto = new RefreshTokenRequestDto { RefreshToken = "unknown-raw-token" };

        _tokenHasher.Setup(h => h.Hash(dto.RefreshToken)).Returns("unknown-hash");

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync("unknown-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.RefreshAsync(dto));
        Assert.Equal("Invalid refresh token.", exception.Message);
    }

    [Fact]
    public async Task RefreshAsync_ShouldRevokeAllActiveTokens_WhenAnAlreadyRevokedTokenIsReused()
    {
        const string tokenHash = "reused-hash";
        var dto = new RefreshTokenRequestDto { RefreshToken = "reused-raw-token" };

        var storedToken = GetRefreshTokenFaker(tokenHash: tokenHash, revokedAt: DateTime.UtcNow.AddHours(-1)).Generate();
        var otherActiveToken = GetRefreshTokenFaker(tokenHash: "another-active-hash").Generate();
        otherActiveToken.UserId = storedToken.UserId;

        _tokenHasher.Setup(h => h.Hash(dto.RefreshToken)).Returns(tokenHash);

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _refreshTokenRepository
            .Setup(r => r.GetActiveByUserIdAsync(storedToken.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([otherActiveToken]);

        _refreshTokenRepository
            .Setup(r => r.Revoke(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => token.RevokedAt = DateTime.UtcNow);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.RefreshAsync(dto));
        Assert.Equal("This session is no longer valid. Please log in again.", exception.Message);

        Assert.NotNull(otherActiveToken.RevokedAt);
        _jwtProvider.Verify(j => j.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task RefreshAsync_ShouldThrow_WhenTokenIsExpired()
    {
        const string tokenHash = "expired-hash";
        var dto = new RefreshTokenRequestDto { RefreshToken = "expired-raw-token" };

        var storedToken = GetRefreshTokenFaker(tokenHash: tokenHash, expiresAt: DateTime.UtcNow.AddDays(-1)).Generate();

        _tokenHasher.Setup(h => h.Hash(dto.RefreshToken)).Returns(tokenHash);

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        var exception = await Assert.ThrowsAsync<DomainException>(() => _sut.RefreshAsync(dto));
        Assert.Equal("This session has expired. Please log in again.", exception.Message);
    }

    // ===================== LOGOUT =====================

    [Fact]
    public async Task LogoutAsync_ShouldRevokeToken_WhenTokenIsValidAndNotRevoked()
    {
        const string tokenHash = "valid-hash";
        var dto = new LogoutDto { RefreshToken = "raw-token" };
        var storedToken = GetRefreshTokenFaker(tokenHash: tokenHash).Generate();

        _tokenHasher.Setup(h => h.Hash(dto.RefreshToken)).Returns(tokenHash);

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        _refreshTokenRepository
            .Setup(r => r.Revoke(It.IsAny<RefreshToken>()))
            .Callback<RefreshToken>(token => token.RevokedAt = DateTime.UtcNow);

        await _sut.LogoutAsync(dto);

        Assert.NotNull(storedToken.RevokedAt);
        _refreshTokenRepository.Verify(r => r.Revoke(It.IsAny<RefreshToken>()), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_ShouldDoNothing_WhenTokenIsNotFound()
    {
        var dto = new LogoutDto { RefreshToken = "unknown-raw-token" };

        _tokenHasher.Setup(h => h.Hash(dto.RefreshToken)).Returns("unknown-hash");

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync("unknown-hash", It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        await _sut.LogoutAsync(dto);

        _refreshTokenRepository.Verify(r => r.Revoke(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    public async Task LogoutAsync_ShouldDoNothing_WhenTokenIsAlreadyRevoked()
    {
        const string tokenHash = "already-revoked-hash";
        var dto = new LogoutDto { RefreshToken = "raw-token" };

        var storedToken = GetRefreshTokenFaker(tokenHash: tokenHash, revokedAt: DateTime.UtcNow.AddHours(-1)).Generate();

        _tokenHasher.Setup(h => h.Hash(dto.RefreshToken)).Returns(tokenHash);

        _refreshTokenRepository
            .Setup(r => r.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(storedToken);

        await _sut.LogoutAsync(dto);

        _refreshTokenRepository.Verify(r => r.Revoke(It.IsAny<RefreshToken>()), Times.Never);
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
        .RuleFor(e => e.CurrentSalary, f => f.Random.Decimal(10000, 50000))
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

    private static Faker<UserProvisionDto> GetProvisionDtoFaker(int employeeId) => new Faker<UserProvisionDto>()
        .RuleFor(d => d.EmployeeId, employeeId)
        .RuleFor(d => d.Username, f => f.Internet.UserName())
        .RuleFor(d => d.Role, f => f.PickRandom(SystemRole.Employee, SystemRole.HR))
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