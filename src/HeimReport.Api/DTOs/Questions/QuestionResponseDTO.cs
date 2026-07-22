using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Questions;

public record QuestionResponseDto
{
    public int Id { get; init; }
    public required int SurveyTemplateId { get; init; }
    public required string Text { get; init; }
    public required QuestionType QuestionType { get; init; }
    public required int OrderIndex { get; init; }
}