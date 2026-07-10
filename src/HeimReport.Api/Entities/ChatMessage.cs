using HeimReport.Api.Enums;

namespace HeimReport.Api.Entities;

public class ChatMessage
{
    public int Id { get; set; }
    public required int ChatSessionId { get; set; }
    public ChatSession? ChatSession { get; set; }
    public required ChatMessageSender Sender { get; set; }
    public required string Content { get; set; }
    public required DateTime SentAt { get; set; }
    public float? SentimentScore { get; set; }
    public int? QuestionId { get; set; }
    public Question? Question { get; set; }
}