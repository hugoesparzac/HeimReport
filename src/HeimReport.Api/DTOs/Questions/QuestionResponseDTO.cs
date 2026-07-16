using HeimReport.Api.Entities;
using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Questions;

public record QuestionsResponseDto
{
    public int Id { get; init; }
    public required int SurveyTemplateId { get; init; }
    public SurveyTemplate? SurveyTemplate { get; init; }
    public required string Text { get; init; }
    public required QuestionType QuestionType { get; init; }
    public required int OrderIndex { get; init; }
}

