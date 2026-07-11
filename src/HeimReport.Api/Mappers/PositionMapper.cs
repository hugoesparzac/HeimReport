using HeimReport.Api.DTOs.Positions;
using HeimReport.Api.Entities;

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
            IsCritical = position.IsCritical
        };
    }
    
    public static Position ToEntity(this PositionCreateUpdateDto dto)
    {
        return new Position
        {
            Title = dto.Title,
            CareerLevel = dto.CareerLevel,
            IsCritical = dto.IsCritical
        };
    }
}