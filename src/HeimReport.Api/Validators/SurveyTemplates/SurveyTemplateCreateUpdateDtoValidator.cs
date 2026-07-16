using FluentValidation;
using HeimReport.Api.DTOs.SurveyTemplates;

namespace HeimReport.Api.Validators.SurveyTemplates;

public class SurveyTemplateCreateUpdateDtoValidator : AbstractValidator<SurveyTemplateCreateUpdateDto>
{
    public SurveyTemplateCreateUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters");
        RuleFor(x => x.MilestoneMonths)
            .NotEmpty().WithMessage("Milestone Months is required")
            .GreaterThan(0).WithMessage("Milestone Months must be a positive integer");
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters");
    }
}

