using HeimReport.Api.DTOs.Questions;
using HeimReport.Api.Entities;

namespace HeimReport.Api.Mappers;

public static class QuestionMapper
{
    public static Question ToEntity(this QuestionCreateDto dto)
    {
        return new Question
        {
            SurveyTemplateId = dto.SurveyTemplateId,
            Text = dto.Text,
            QuestionType = dto.QuestionType,
            OrderIndex = dto.OrderIndex
        };
    }

    public static QuestionResponseDto ToResponseDto(this Question question)
    {
        return new QuestionResponseDto
        {
            Id = question.Id,
            SurveyTemplateId = question.SurveyTemplateId,
            Text = question.Text,
            QuestionType = question.QuestionType,
            OrderIndex = question.OrderIndex
        };
    }

    public static void ApplyUpdate(this Question question, QuestionUpdateDto dto)
    {
        question.Text = dto.Text;
        question.OrderIndex = dto.OrderIndex;
        question.QuestionType = dto.QuestionType;
    }
}