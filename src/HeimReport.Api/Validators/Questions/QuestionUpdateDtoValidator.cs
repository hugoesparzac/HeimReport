using FluentValidation;
using HeimReport.Api.DTOs.Questions;

namespace HeimReport.Api.Validators.Questions;
public class QuestionUpdateDtoValidator: AbstractValidator<QuestionUpdateDto>
{
    public QuestionUpdateDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(500).WithMessage("Text must not exceed 500 characters");
        RuleFor(x => x.OrderIndex)
            .NotEmpty().WithMessage("OrderIndex is required")
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be a non-negative integer");
    }
}

