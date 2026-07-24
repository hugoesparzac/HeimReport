using HeimReport.Api.DTOs.Positions;

namespace HeimReport.Api.Services.Positions;

public interface IPositionService
{
    Task<IEnumerable<PositionResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PositionResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PositionResponseDto> CreateAsync(PositionCreateUpdateDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, PositionCreateUpdateDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}