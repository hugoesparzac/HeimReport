using HeimReport.Api.Enums;

namespace HeimReport.Api.Entities;

public class SurveyInstance
{
    public int Id { get; set; }
    public required int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public required int SurveyTemplateId { get; set; }
    public SurveyTemplate? SurveyTemplate { get; set; }
    public required DateTime ScheduledDate { get; set; }
    public DateTime? DueDate { get; set; }
    public required SurveyInstanceStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public required SurveyChannel Channel { get; set; }
}