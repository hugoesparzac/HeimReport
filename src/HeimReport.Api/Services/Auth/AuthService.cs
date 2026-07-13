using HeimReport.Api.DTOs.Auth;
using HeimReport.Api.Email;
using HeimReport.Api.Entities;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.Auth;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Security;
using Microsoft.Extensions.Options;

namespace HeimReport.Api.Services.Auth;

public sealed partial class AuthService(
    IEmployeeRepository employeeRepository,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenHasher tokenHasher,
    IJwtProvider jwtProvider,
    IEmailSender emailSender,
    IOptions<JwtOptions> jwtOptions,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private static readonly TimeSpan VerificationTokenLifetime = TimeSpan.FromHours(24);

    public async Task RegisterAsync(UserRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToUpperInvariant();

        var employee = await employeeRepository.GetActiveByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (employee is null)
        {
            LogNoMatchingEmployeeForRegistration(dto.Email);
            throw new DomainException("Unable to complete registration with the provided information.");
        }

        var existingUser = await userRepository.GetByEmployeeIdAsync(employee.Id, cancellationToken);
        if (existingUser is not null)
        {
            LogEmployeeAlreadyHasAccount(employee.Id);
            throw new DomainException("Unable to complete registration with the provided information.");
        }

        var normalizedUsername = dto.Username.Trim().ToUpperInvariant();
        var usernameTaken = await userRepository.GetByNormalizedUsernameAsync(normalizedUsername, cancellationToken);
        if (usernameTaken is not null)
        {
            throw new DomainException("This username is already taken. Please choose a different one.");
        }

        var user = dto.ToEntity(employee.Id);
        user.PasswordHash = passwordHasher.Hash(dto.Password);

        var rawToken = tokenHasher.GenerateRawToken();
        var tokenHash = tokenHasher.Hash(rawToken);
        user.SetEmailVerificationToken(tokenHash, DateTime.UtcNow.Add(VerificationTokenLifetime));

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        await emailSender.SendEmailVerificationAsync(
            employee.Email,
            rawToken,
            user.PreferredLanguage,
            cancellationToken);

        LogUserRegistered(employee.Id);
    }

    public async Task VerifyEmailAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenHasher.Hash(rawToken);
        var user = await userRepository.GetByEmailVerificationTokenHashAsync(tokenHash, cancellationToken);

        if (user is null)
        {
            throw new DomainException("This verification link is invalid or has already been used.");
        }

        if (user.EmailVerificationTokenExpiresAt is null || user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
        {
            throw new DomainException("This verification link has expired. Please request a new one.");
        }

        user.ConfirmEmailVerification();
        await userRepository.SaveChangesAsync(cancellationToken);

        LogEmailVerified(user.Id);
    }

    public async Task<TokenResponseDto> LoginAsync(UserLoginDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedInput = dto.UsernameOrEmail.Trim().ToUpperInvariant();
        var user = await userRepository.GetByUsernameOrEmailAsync(normalizedInput, cancellationToken);

        if (user is null || !passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            LogFailedLoginAttempt(dto.UsernameOrEmail);
            throw new DomainException("Invalid username/email or password.");
        }

        if (!user.IsActive)
        {
            throw new DomainException("This account has been deactivated. Please contact support.");
        }

        if (!user.IsEmailVerified)
        {
            throw new DomainException("Please verify your email address before logging in.");
        }

        var accessToken = jwtProvider.GenerateToken(user);

        var rawRefreshToken = tokenHasher.GenerateRawToken();
        var refreshTokenHash = tokenHasher.Hash(rawRefreshToken);
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationInDays);

        RefreshToken refreshToken = new()
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        user.RecordLogin();

        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        LogUserLoggedIn(user.Id);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes)
        };
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Registration attempted for an email with no matching active employee: {Email}")]
    private partial void LogNoMatchingEmployeeForRegistration(string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Registration attempted for an employee that already has an account: EmployeeId {EmployeeId}")]
    private partial void LogEmployeeAlreadyHasAccount(int employeeId);

    [LoggerMessage(Level = LogLevel.Information, Message = "User registered successfully for EmployeeId {EmployeeId}")]
    private partial void LogUserRegistered(int employeeId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Email verified successfully for UserId {UserId}")]
    private partial void LogEmailVerified(int userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed login attempt for input: {Input}")]
    private partial void LogFailedLoginAttempt(string input);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} logged in successfully")]
    private partial void LogUserLoggedIn(int userId);
}
