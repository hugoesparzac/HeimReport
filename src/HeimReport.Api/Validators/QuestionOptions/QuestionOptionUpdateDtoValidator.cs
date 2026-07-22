using FluentValidation;
using HeimReport.Api.DTOs.QuestionOption;

namespace HeimReport.Api.Validators.QuestionOptions;

public class QuestionOptionUpdateDtoValidator : AbstractValidator<QuestionOptionUpdateDto>
{
    public QuestionOptionUpdateDtoValidator()
    {
        RuleFor(x => x.QuestionId)
            .GreaterThan(0).WithMessage("QuestionId must be a positive integer");
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(200).WithMessage("Text must not exceed 200 characters");
        RuleFor(x => x.Value)
            .MaximumLength(50).WithMessage("Value must not exceed 50 characters");
        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be a non-negative integer");
    }
}