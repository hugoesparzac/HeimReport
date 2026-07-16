using HeimReport.Api.Entities;
using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Questions;

public record QuestionsUpdateDTO
{
    public required int Id { get; set; }
    public required string Text { get; init; }
    public required QuestionType QuestionType { get; init; }
    public required int OrderIndex { get; init; }
}

