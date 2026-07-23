using FluentValidation;
using HeimReport.Api.DTOs.SurveyTemplates;

namespace HeimReport.Api.Validators.SurveyTemplates;

public class QuestionOptionCreateNestedDtoValidator : AbstractValidator<QuestionOptionCreateNestedDto>
{
    public QuestionOptionCreateNestedDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Option text is required")
            .MaximumLength(200).WithMessage("Option text must not exceed 200 characters");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be a non-negative integer");
    }
}