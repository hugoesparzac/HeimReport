using HeimReport.Api.DTOs.Auth;

namespace HeimReport.Api.Services.Auth;

public interface IAuthService
{
    Task RegisterAsync(UserRegistrationDto dto, CancellationToken cancellationToken = default);
    Task VerifyEmailAsync(string rawToken, CancellationToken cancellationToken = default);
}
