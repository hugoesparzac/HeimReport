namespace HeimReport.Api.DTOs.QuestionOption;

public record QuestionOptionUpdateDTO
{
    public required int Id { get; init; }
    public required int QuestionId { get; init; }
    public required string Text { get; init; }
    public string? Value { get; init; }
    public required int OrderIndex { get; init; }
}

