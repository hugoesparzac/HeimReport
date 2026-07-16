using HeimReport.Api.Entities;
using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Questions;

public record QuestionsResponseDTO
{
    public required int Id { get; set; }
    public required int SurveyTemplateId { get; init; }
    public SurveyTemplate? SurveyTemplate { get; init; }
    public required string Text { get; init; }
    public required QuestionType QuestionType { get; init; }
    public required int OrderIndex { get; init; }
}

