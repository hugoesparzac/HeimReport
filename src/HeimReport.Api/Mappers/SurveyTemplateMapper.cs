using HeimReport.Api.DTOs.SurveyTemplates;
using HeimReport.Api.Entities;
using HeimReport.Api.Mappers.Questions;

namespace HeimReport.Api.Mappers;
public static class SurveyTemplateMapper
{
    public static SurveyTemplateResponseDT ToResponseDto(this SurveyTemplate surveyTemplate)
    {
        return new SurveyTemplateResponseDT
        {
            Id = surveyTemplate.Id,
            Name = surveyTemplate.Name,
            Description = surveyTemplate.Description,
            Questions = surveyTemplate.Questions?.Select(q => q.ToResponseDto()).ToList() ?? new List<QuestionResponseDto>()
        };
    }
}

