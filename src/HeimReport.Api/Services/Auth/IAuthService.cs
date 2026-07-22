using HeimReport.Api.DTOs.Auth;

namespace HeimReport.Api.Services.Auth;

public interface IAuthService
{
    Task<UserResponseDto> RegisterAsync(UserRegistrationDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task VerifyEmailAsync(string rawToken, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> LoginAsync(UserLoginDto dto, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> RefreshAsync(string rawRefreshToken, CancellationToken cancellationToken = default);
    Task LogoutAsync(string rawRefreshToken, CancellationToken cancellationToken = default);
    Task ResendVerificationAsync(ResendEmailVerificationDto dto, CancellationToken cancellationToken = default);
}
