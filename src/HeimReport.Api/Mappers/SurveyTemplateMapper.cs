using HeimReport.Api.DTOs.SurveyTemplates;
using HeimReport.Api.Entities;

namespace HeimReport.Api.Mappers;

public static class SurveyTemplateMapper
{
    public static SurveyTemplate ToEntity(this SurveyTemplateCreateUpdateDto dto)
    {
        return new SurveyTemplate
        {
            Name = dto.Name,
            MilestoneMonths = dto.MilestoneMonths,
            Description = dto.Description,
            IsActive = dto.IsActive
        };
    }

    public static SurveyTemplateResponseDto ToResponseDto(this SurveyTemplate template)
    {
        return new SurveyTemplateResponseDto
        {
            Id = template.Id,
            Name = template.Name,
            MilestoneMonths = template.MilestoneMonths,
            Description = template.Description,
            IsActive = template.IsActive,
            CreatedAt = template.CreatedAt
        };
    }
}