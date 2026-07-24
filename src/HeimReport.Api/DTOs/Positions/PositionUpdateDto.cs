using HeimReport.Api.Enums;
namespace HeimReport.Api.DTOs.Positions;

public record PositionUpdateDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required CareerLevel CareerLevel { get; init; }
    public required bool IsCritical { get; init; }
    public bool IsActive { get; init; }
}