namespace HeimReport.Api.Entities;

public class Answer
{
    public int Id { get; set; }
    public required int SurveyInstanceId { get; set; }
    public SurveyInstance? SurveyInstance { get; set; }
    public required int QuestionId { get; set; }
    public Question? Question { get; set; }
    public string? RawText { get; set; }
    public string? NormalizedText { get; set; }
    public decimal? SentimentScore { get; set; }
    public int? ChatMessageId { get; set; }
    public required DateTime AnsweredAt { get; set; }
}