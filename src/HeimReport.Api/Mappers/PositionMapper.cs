using HeimReport.Api.Entities;
using HeimReport.Api.DTOs.Positions;

namespace HeimReport.Api.Mappers;

public static class PositionMapper
{
    public static PositionResponseDto ToResponseDto(this Position position)
    {
        return new PositionResponseDto
        {
            Id = position.Id,
            Title = position.Title,
            CareerLevel = position.CareerLevel,
            IsCritical = position.IsCritical,
            IsActive = position.IsActive
        };
    }

    public static Position ToEntity(this PositionCreateDto dto)
    {
        return new Position
        {
            Title = dto.Title,
            CareerLevel = dto.CareerLevel,
            IsCritical = dto.IsCritical,
            IsActive = dto.IsActive
        };
    }

    public static void UpdateEntity(this PositionUpdateDto dto, Position position)
    {
        position.Title = dto.Title;
        position.CareerLevel = dto.CareerLevel;
        position.IsCritical = dto.IsCritical;
        position.IsActive = dto.IsActive;
    }
}