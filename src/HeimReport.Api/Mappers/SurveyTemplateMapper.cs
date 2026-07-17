
using HeimReport.Api.DTOs.SurveyTemplates;
using HeimReport.Api.Entities;

namespace HeimReport.Api.Mappers;

public static class SurveyTemplateMapper
{

    public static SurveyTemplate ToEntity(this SurveyTemplateCreateUpdateDto SurveyTemplateDto)
    {
        return new SurveyTemplate
        {
            Name = SurveyTemplateDto.Name,
            MilestoneMonths = SurveyTemplateDto.MilestoneMonths,
            Description = SurveyTemplateDto.Description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static SurveyTemplateResponseDto ToResponseDto(this SurveyTemplate SurveyTemplate)
    {
        return new SurveyTemplateResponseDto
        {
            Id = SurveyTemplate.Id,
            Name = SurveyTemplate.Name,
            MilestoneMonths = SurveyTemplate.MilestoneMonths,
            Description = SurveyTemplate.Description,
            IsActive = SurveyTemplate.IsActive,
            CreatedAt = SurveyTemplate.CreatedAt
        };
    }
}