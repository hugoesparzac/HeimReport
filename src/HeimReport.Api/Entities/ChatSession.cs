using HeimReport.Api.Enums;

namespace HeimReport.Api.Entities;

public class ChatSession
{
    public int Id { get; set; }
    public required int SurveyInstanceId { get; set; }
    public SurveyInstance? SurveyInstance { get; set; }
    public required DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public required ChatSessionStatus Status { get; set; }
}