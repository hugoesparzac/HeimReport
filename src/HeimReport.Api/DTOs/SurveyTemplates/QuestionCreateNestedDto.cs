using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.SurveyTemplates;

public record QuestionCreateNestedDto
{
    public required string Text { get; init; }
    public required QuestionType QuestionType { get; init; }
    public required int OrderIndex { get; init; }

    public List<QuestionOptionCreateNestedDto> Options { get; init; } = [];
}

