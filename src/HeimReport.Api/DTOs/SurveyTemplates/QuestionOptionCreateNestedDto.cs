namespace HeimReport.Api.DTOs.SurveyTemplates;

public record QuestionOptionCreateNestedDto
{
    public required string Text { get; init; }
    public required int OrderIndex { get; init; }
}