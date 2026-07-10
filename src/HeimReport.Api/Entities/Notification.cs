using HeimReport.Api.Enums;

namespace HeimReport.Api.Entities;

public class Notification
{
    public int Id { get; set; }
    public required int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public int? SurveyInstanceId { get; set; }
    public SurveyInstance? SurveyInstance { get; set; }
    public required NotificationType Type { get; set; }
    public required DateTime SentAt { get; set; }
    public required NotificationChannel Channel { get; set; }
}