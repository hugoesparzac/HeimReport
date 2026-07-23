using HeimReport.Api.DTOs.Users;
using HeimReport.Api.Email;
using HeimReport.Api.Entities;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.Users;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Security;
using Microsoft.Extensions.Options;

namespace HeimReport.Api.Services.Users;

public sealed partial class UserService(
    IEmployeeRepository employeeRepository,
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    ITokenHasher tokenHasher,
    IJwtProvider jwtProvider,
    IEmailSender emailSender,
    IOptions<JwtOptions> jwtOptions,
    ILogger<UserService> logger) : IUserService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private static readonly TimeSpan VerificationTokenLifetime = TimeSpan.FromHours(24);

    public async Task<UserResponseDto> RegisterAsync(UserRegistrationDto dto, CancellationToken cancellationToken = default)
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

        return user.ToResponseDto();
    }

    public async Task<UserResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<User>(id);

        return user.ToResponseDto();
    }

    public async Task VerifyEmailAsync(string rawToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenHasher.Hash(rawToken);

        var user = await userRepository.GetByEmailVerificationTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new DomainException("This verification link is invalid or has already been used.");

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

    public async Task<TokenResponseDto> RefreshAsync(string rawRefreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenHasher.Hash(rawRefreshToken);

        var storedToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new DomainException("Invalid refresh token.");

        if (storedToken.RevokedAt is not null)
        {
            LogRevokedTokenReuseDetected(storedToken.UserId);

            var activeTokens = await refreshTokenRepository.GetActiveByUserIdAsync(storedToken.UserId, cancellationToken);
            foreach (var token in activeTokens)
            {
                refreshTokenRepository.Revoke(token);
            }
            await refreshTokenRepository.SaveChangesAsync(cancellationToken);

            throw new DomainException("This session is no longer valid. Please log in again.");
        }

        if (storedToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new DomainException("This session has expired. Please log in again.");
        }

        if (storedToken.User?.Employee is null)
        {
            throw new InvalidOperationException(
                $"Cannot generate token: RefreshToken with Id {storedToken.Id} was loaded without its related User/Employee. " +
                "Ensure the query includes .Include(rt => rt.User).ThenInclude(u => u.Employee).");
        }

        var newAccessToken = jwtProvider.GenerateToken(storedToken.User);
        var newRawRefreshToken = tokenHasher.GenerateRawToken();
        var newTokenHash = tokenHasher.Hash(newRawRefreshToken);
        var newExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationInDays);

        var newRefreshToken = new RefreshToken
        {
            UserId = storedToken.UserId,
            TokenHash = newTokenHash,
            ExpiresAt = newExpiresAt,
            CreatedAt = DateTime.UtcNow
        };

        await refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);

        storedToken.ReplacedByTokenHash = newTokenHash;
        refreshTokenRepository.Revoke(storedToken);

        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        LogRefreshTokenRotated(storedToken.UserId);

        return new TokenResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRawRefreshToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes)
        };
    }

    public async Task LogoutAsync(string rawRefreshToken, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenHasher.Hash(rawRefreshToken);
        var storedToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (storedToken is null || storedToken.RevokedAt is not null)
        {
            return;
        }

        refreshTokenRepository.Revoke(storedToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        LogUserLoggedOut(storedToken.UserId);
    }

    public async Task ResendVerificationAsync(ResendEmailVerificationDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToUpperInvariant();
        var employee = await employeeRepository.GetActiveByNormalizedEmailAsync(normalizedEmail, cancellationToken);

        if (employee is null)
        {
            LogNonMatchingEmailForResend(dto.Email);
            return;
        }

        var user = await userRepository.GetByEmployeeIdAsync(employee.Id, cancellationToken);
        if (user?.IsEmailVerified is not false)
        {
            return;
        }

        var rawToken = tokenHasher.GenerateRawToken();
        var tokenHash = tokenHasher.Hash(rawToken);
        user.SetEmailVerificationToken(tokenHash, DateTime.UtcNow.Add(VerificationTokenLifetime));

        await userRepository.SaveChangesAsync(cancellationToken);

        await emailSender.SendEmailVerificationAsync(
            employee.Email,
            rawToken,
            user.PreferredLanguage,
            cancellationToken);

        LogVerificationEmailResent(employee.Id);
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

    [LoggerMessage(Level = LogLevel.Warning, Message = "Detected reuse of a revoked refresh token for UserId {UserId}. Revoking all active sessions.")]
    private partial void LogRevokedTokenReuseDetected(int userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Refresh token rotated successfully for UserId {UserId}")]
    private partial void LogRefreshTokenRotated(int userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} logged out successfully")]
    private partial void LogUserLoggedOut(int userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Resend verification requested for a non-matching email: {Email}")]
    private partial void LogNonMatchingEmailForResend(string email);

    [LoggerMessage(Level = LogLevel.Information, Message = "Verification email resent for EmployeeId {EmployeeId}")]
    private partial void LogVerificationEmailResent(int employeeId);
}
