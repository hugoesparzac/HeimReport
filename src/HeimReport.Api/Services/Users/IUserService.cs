using HeimReport.Api.DTOs.Common;
using HeimReport.Api.DTOs.Users;

namespace HeimReport.Api.Services.Users;

public interface IUserService
{
    Task<UserResponseDto> RegisterAsync(UserRegistrationDto dto, CancellationToken cancellationToken = default);
    Task<UserResponseDto> ProvisionAsync(UserProvisionDto dto, CancellationToken cancellationToken = default);
    Task<PagedResultDto<UserResponseDto>> GetPagedAsync(UserQueryDto query, CancellationToken cancellationToken = default);
    Task<UserResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UserUpdateDto dto, CancellationToken cancellationToken = default);
    Task ChangePasswordAsync(int userId, ChangePasswordDto dto, CancellationToken cancellationToken = default);
    Task VerifyEmailAsync(VerifyEmailDto dto, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> LoginAsync(UserLoginDto dto, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> RefreshAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default);
    Task LogoutAsync(LogoutDto dto, CancellationToken cancellationToken = default);
    Task ResendVerificationAsync(ResendEmailVerificationDto dto, CancellationToken cancellationToken = default);
}