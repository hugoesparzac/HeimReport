using FluentValidation;
using HeimReport.Api.DTOs.SurveyTemplates;

namespace HeimReport.Api.Validators.SurveyTemplates;

public class QuestionCreateNestedDtoValidator : AbstractValidator<QuestionCreateNestedDto>
{
    public QuestionCreateNestedDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Question text is required")
            .MaximumLength(500).WithMessage("Question text must not exceed 500 characters");

        RuleFor(x => x.QuestionType)
            .IsInEnum().WithMessage("Invalid Question Type");

        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be a non-negative integer");

        RuleForEach(x => x.Options)
            .SetValidator(new QuestionOptionCreateNestedDtoValidator());
    }
}