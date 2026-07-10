namespace HeimReport.Api.Entities;

public class QuestionOption
{
    public int Id { get; set; }
    public required int QuestionId { get; set; }
    public Question? Question { get; set; }
    public required string Text { get; set; }
    public string? Value { get; set; }
    public required int OrderIndex { get; set; }
}