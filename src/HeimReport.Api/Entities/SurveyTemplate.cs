namespace HeimReport.Api.Entities;

public class SurveyTemplate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required int MilestoneMonths { get; set; }
    public string? Description { get; set; }
    public required bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}