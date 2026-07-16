namespace HeimReport.Api.DTOs.SurveyTemplates
{
    public record SurveyTemplateCreateUpdateDTO
    {
        public required string Name { get; init; }
        public required int MilestoneMonths { get; init; }
        public string? Description { get; init; }
        public required bool IsActive { get; init; }
        public required DateTime CreatedAt { get; init; }
    }
}
