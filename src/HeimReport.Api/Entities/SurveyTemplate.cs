namespace HeimReport.Api.Entities;

public class SurveyTemplate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required int MilestoneMonths { get; set; } // Months since HireDate that apply: 1, 3, 6, 12...
    public string? Description { get; set; }
    public required bool IsActive { get; set; }
    public required DateTime CreatedAt { get; set; }
}