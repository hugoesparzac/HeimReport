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
        RuleFor(x => x.QuestionType)
            .IsInEnum().WithMessage("QuestionType must be a valid enum value");
        RuleFor(x => x.OrderIndex)
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be a non-negative integer");
    }
}