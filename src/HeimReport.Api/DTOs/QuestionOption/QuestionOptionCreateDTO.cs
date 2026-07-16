namespace HeimReport.Api.DTOs.QuestionOption;
public record QuestionOptionCreateDTO
{
    public required int QuestionId { get; set; }
    public required string Text { get; set; }
    public string? Value { get; set; }
    public required int OrderIndex { get; set; }

}

