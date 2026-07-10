using HeimReport.Api.Enums;

namespace HeimReport.Api.Entities;

public class Question
{
    public int Id { get; set; }
    public required int SurveyTemplateId { get; set; }
    public SurveyTemplate? SurveyTemplate { get; set; }
    public required string Text { get; set; }
    public required QuestionType QuestionType { get; set; }
    public required int OrderIndex { get; set; }
}