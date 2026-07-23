using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.QuestionOption;

public record QuestionOptionResponseDto
{
    public int Id { get; init; }
    public required int QuestionId { get; init; }
    public required string Text { get; init; }
    public string? Value { get; init; }
    public required int OrderIndex { get; init; }
}