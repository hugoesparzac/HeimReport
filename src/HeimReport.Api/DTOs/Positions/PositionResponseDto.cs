using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Positions;

public record PositionResponseDto(int Id, string Title, CareerLevel CareerLevel, bool IsCritical);