using HeimReport.Api.DTOs.Auth;
using HeimReport.Api.Email;
using HeimReport.Api.Exceptions;
using HeimReport.Api.Mappers;
using HeimReport.Api.Repositories.Auth;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Security;

namespace HeimReport.Api.Services.Auth;

public sealed class AuthService(
    IEmployeeRepository employeeRepository,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenHasher tokenHasher,
    IEmailSender emailSender,
    ILogger<AuthService> logger) : IAuthService
{
    private static readonly TimeSpan VerificationTokenLifetime = TimeSpan.FromHours(24);

    public async Task RegisterAsync(UserRegistrationDto dto, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToUpperInvariant();

        var employee = await employeeRepository.GetActiveByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (employee is null)
        {
            logger.LogWarning("Registration attempted for an email with no matching active employee: {Email}", dto.Email);
            throw new DomainException("Unable to complete registration with the provided information.");
        }

        var existingUser = await userRepository.GetByEmployeeIdAsync(employee.Id, cancellationToken);
        if (existingUser is not null)
        {
            logger.LogWarning("Registration attempted for an employee that already has an account: EmployeeId {EmployeeId}", employee.Id);
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

        logger.LogInformation("User registered successfully for EmployeeId {EmployeeId}", employee.Id);
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

        logger.LogInformation("Email verified successfully for UserId {UserId}", user.Id);
    }
}
