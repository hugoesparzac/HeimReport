namespace HeimReport.Api.DTOs.SurveyTemplates;

public record SurveyTemplateWithQuestionsDto
{
    public required string Name { get; init; }
    public required int MilestoneMonths { get; init; }
    public string? Description { get; init; }
    public required bool IsActive { get; init; }

    public required List<QuestionCreateNestedDto> Questions { get; init; }
}

