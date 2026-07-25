using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Users;
using HeimReport.Api.Email;
using HeimReport.Api.Entities;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Repositories.Users;
using HeimReport.Api.Security;
using HeimReport.Api.Services.AuditLogs;
using Microsoft.EntityFrameworkCore;
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
    IAuditLogService auditLogService,
    IOptions<JwtOptions> jwtOptions,
    ILogger<UserService> logger) : IUserService
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;
    private static readonly TimeSpan VerificationTokenLifetime = TimeSpan.FromHours(24);

    // ===================== REGISTRATION =====================

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

        var passwordHash = passwordHasher.Hash(dto.Password);
        var user = dto.ToEntity(employee.Id, passwordHash);

        var rawToken = tokenHasher.GenerateRawToken();
        user.EmailVerificationTokenHash = tokenHasher.Hash(rawToken);
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.Add(VerificationTokenLifetime);

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        await emailSender.SendEmailVerificationAsync(employee.Email, rawToken, user.PreferredLanguage, cancellationToken);

        LogUserRegistered(employee.Id);

        await auditLogService.LogAsync(
            "REGISTER_USER", nameof(User), user.Id,
            oldValues: null,
            newValues: new { user.EmployeeId, user.Username, user.Role },
            cancellationToken: cancellationToken);

        var created = await userRepository.GetByIdWithDetailsAsync(user.Id, cancellationToken)
            ?? throw NotFoundException.ForEntity<User>(user.Id);

        return created.ToResponseDto();
    }

    // ===================== PROVISIONING =====================

    public async Task<UserResponseDto> ProvisionAsync(UserProvisionDto dto, CancellationToken cancellationToken = default)
    {
        var employee = await employeeRepository.GetByIdAsync(dto.EmployeeId, cancellationToken)
            ?? throw NotFoundException.ForEntity<Employee>(dto.EmployeeId);

        var temporaryPassword = passwordHasher.GenerateTemporaryPassword();
        var passwordHash = passwordHasher.Hash(temporaryPassword);

        var user = dto.ToEntity(passwordHash);

        var rawToken = tokenHasher.GenerateRawToken();
        user.EmailVerificationTokenHash = tokenHasher.Hash(rawToken);
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.Add(VerificationTokenLifetime);

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        await emailSender.SendTemporaryPasswordAsync(employee.Email, temporaryPassword, user.PreferredLanguage, cancellationToken);
        await emailSender.SendEmailVerificationAsync(employee.Email, rawToken, user.PreferredLanguage, cancellationToken);

        LogUserProvisioned(employee.Id, dto.Role);

        await auditLogService.LogAsync(
            "PROVISION_USER", nameof(User), user.Id,
            oldValues: null,
            newValues: new { user.EmployeeId, user.Username, user.Role, user.PreferredLanguage },
            cancellationToken: cancellationToken);

        var created = await userRepository.GetByIdWithDetailsAsync(user.Id, cancellationToken)
            ?? throw NotFoundException.ForEntity<User>(user.Id);

        return created.ToResponseDto();
    }

    // ===================== READ =====================

    public async Task<PagedResultDto<UserResponseDto>> GetPagedAsync(UserQueryDto query, CancellationToken cancellationToken = default)
    {
        var baseQuery = userRepository.QueryWithDetails();

        if (!string.IsNullOrWhiteSpace(query.SearchText))
        {
            var pattern = $"%{query.SearchText}%";
            baseQuery = baseQuery.Where(u =>
                EF.Functions.ILike(u.Username, pattern) ||
                EF.Functions.ILike(u.Employee!.FirstName, pattern) ||
                EF.Functions.ILike(u.Employee!.LastName, pattern) ||
                EF.Functions.ILike(u.Employee!.Email, pattern));
        }

        if (query.Role.HasValue)
        {
            baseQuery = baseQuery.Where(u => u.Role == query.Role.Value);
        }

        if (query.IsActive.HasValue)
        {
            baseQuery = baseQuery.Where(u => u.IsActive == query.IsActive.Value);
        }

        baseQuery = baseQuery.OrderBy(u => u.Username);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var entities = await baseQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<UserResponseDto>
        {
            Items = [.. entities.Select(u => u.ToResponseDto())],
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<UserResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<User>(id);

        return user.ToResponseDto();
    }

    // ===================== UPDATE =====================

    public async Task UpdateAsync(int id, UserUpdateDto dto, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw NotFoundException.ForEntity<User>(id);

        var oldValues = new { user.Role, user.IsActive, user.PreferredLanguage };
        var isDeactivating = user.IsActive && !dto.IsActive;

        dto.UpdateEntity(user);
        userRepository.Update(user);

        if (isDeactivating)
        {
            await refreshTokenRepository.RevokeAllActiveByUserIdAsync(id, cancellationToken);
            LogUserDeactivatedSessionsRevoked(id);
        }

        await userRepository.SaveChangesAsync(cancellationToken);

        await auditLogService.LogAsync(
            "UPDATE_USER", nameof(User), id,
            oldValues,
            new { dto.Role, dto.IsActive, dto.PreferredLanguage },
            cancellationToken);
    }

    // ===================== CHANGE PASSWORD =====================

    public async Task ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken)
            ?? throw NotFoundException.ForEntity<User>(userId);

        if (!passwordHasher.Verify(dto.CurrentPassword, user.PasswordHash))
        {
            throw new DomainException("Current password is incorrect.");
        }

        user.PasswordHash = passwordHasher.Hash(dto.NewPassword);
        userRepository.Update(user);

        await userRepository.SaveChangesAsync(cancellationToken);

        LogPasswordChanged(userId);

        await auditLogService.LogAsync(
            "CHANGE_PASSWORD", nameof(User), userId,
            oldValues: null, newValues: null,
            cancellationToken: cancellationToken);
    }

    // ===================== EMAIL VERIFICATION =====================

    public async Task VerifyEmailAsync(VerifyEmailDto dto, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenHasher.Hash(dto.Token);

        var user = await userRepository.GetByEmailVerificationTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new DomainException("This verification link is invalid or has already been used.");

        if (user.EmailVerificationTokenExpiresAt is null || user.EmailVerificationTokenExpiresAt < DateTime.UtcNow)
        {
            throw new DomainException("This verification link has expired. Please request a new one.");
        }

        user.IsEmailVerified = true;
        user.EmailVerificationTokenHash = null;
        user.EmailVerificationTokenExpiresAt = null;

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);

        LogEmailVerified(user.Id);

        await auditLogService.LogAsync(
            "VERIFY_EMAIL", nameof(User), user.Id,
            cancellationToken: cancellationToken);
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
        if (user?.IsEmailVerified ?? true)
        {
            return;
        }

        var rawToken = tokenHasher.GenerateRawToken();
        user.EmailVerificationTokenHash = tokenHasher.Hash(rawToken);
        user.EmailVerificationTokenExpiresAt = DateTime.UtcNow.Add(VerificationTokenLifetime);

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);

        await emailSender.SendEmailVerificationAsync(employee.Email, rawToken, user.PreferredLanguage, cancellationToken);

        LogVerificationEmailResent(employee.Id);

        await auditLogService.LogAsync(
            "RESEND_VERIFICATION", nameof(User), user.Id,
            cancellationToken: cancellationToken);
    }

    // ===================== LOGIN / REFRESH / LOGOUT =====================

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

        var response = await IssueTokenResponseAsync(user, cancellationToken);

        user.LastLoginAt = DateTime.UtcNow;
        userRepository.Update(user);

        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        LogUserLoggedIn(user.Id);

        await auditLogService.LogAsync("LOGIN", nameof(User), user.Id, cancellationToken: cancellationToken);

        return response;
    }

    public async Task<TokenResponseDto> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenHasher.Hash(dto.RefreshToken);

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

            await auditLogService.LogAsync(
                "REFRESH_TOKEN_REUSE_DETECTED", nameof(User), storedToken.UserId,
                cancellationToken: cancellationToken);

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

        var response = await IssueTokenResponseAsync(storedToken.User, cancellationToken);

        storedToken.ReplacedByTokenHash = tokenHasher.Hash(response.RefreshToken);
        refreshTokenRepository.Revoke(storedToken);

        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        LogRefreshTokenRotated(storedToken.UserId);

        return response;
    }

    public async Task LogoutAsync(LogoutDto dto, CancellationToken cancellationToken = default)
    {
        var tokenHash = tokenHasher.Hash(dto.RefreshToken);
        var storedToken = await refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        if (storedToken is null || storedToken.RevokedAt is not null)
        {
            return;
        }

        refreshTokenRepository.Revoke(storedToken);
        await refreshTokenRepository.SaveChangesAsync(cancellationToken);

        LogUserLoggedOut(storedToken.UserId);

        await auditLogService.LogAsync("LOGOUT", nameof(User), storedToken.UserId, cancellationToken: cancellationToken);
    }

    // ===================== PRIVATE HELPERS =====================

    private async Task<TokenResponseDto> IssueTokenResponseAsync(User user, CancellationToken cancellationToken)
    {
        var accessToken = jwtProvider.GenerateToken(user);

        var rawRefreshToken = tokenHasher.GenerateRawToken();
        var refreshTokenHash = tokenHasher.Hash(rawRefreshToken);

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpirationInDays),
            CreatedAt = DateTime.UtcNow
        };

        await refreshTokenRepository.AddAsync(refreshToken, cancellationToken);

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = rawRefreshToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationInMinutes)
        };
    }

    // ===================== LOGGING =====================

    [LoggerMessage(Level = LogLevel.Warning, Message = "Registration attempted for an email with no matching active employee: {Email}")]
    private partial void LogNoMatchingEmployeeForRegistration(string email);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Registration attempted for an employee that already has an account: EmployeeId {EmployeeId}")]
    private partial void LogEmployeeAlreadyHasAccount(int employeeId);

    [LoggerMessage(Level = LogLevel.Information, Message = "User registered successfully for EmployeeId {EmployeeId}")]
    private partial void LogUserRegistered(int employeeId);

    [LoggerMessage(Level = LogLevel.Information, Message = "User provisioned for EmployeeId {EmployeeId} with role {Role}")]
    private partial void LogUserProvisioned(int employeeId, Enums.SystemRole role);

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

    [LoggerMessage(Level = LogLevel.Information, Message = "Password changed successfully for UserId {UserId}")]
    private partial void LogPasswordChanged(int userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} deactivated; all active sessions revoked")]
    private partial void LogUserDeactivatedSessionsRevoked(int userId);
}