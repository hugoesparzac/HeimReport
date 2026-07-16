using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Positions;

public record PositionCreateUpdateDto
{
    public required string Title { get; init; }
    public required CareerLevel CareerLevel { get; init; }
    public required bool IsCritical { get; init; }
}