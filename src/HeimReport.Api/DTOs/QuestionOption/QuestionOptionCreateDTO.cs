namespace HeimReport.Api.DTOs.QuestionOption;
public record QuestionOptionCreateDto
{
    public required int QuestionId { get; init; }
    public required string Text { get; init; }
    public string? Value { get; init; }
    public required int OrderIndex { get; init; }

}

