using HeimReport.Api.DTOs.Auth;

namespace HeimReport.Api.Services.Auth;

public interface IAuthService
{
    Task RegisterAsync(UserRegistrationDto dto, CancellationToken cancellationToken = default);
    Task VerifyEmailAsync(string rawToken, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> LoginAsync(UserLoginDto dto, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> RefreshAsync(string rawRefreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string rawRefreshToken, CancellationToken cancellationToken = default);
}
