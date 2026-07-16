using FluentValidation;
using HeimReport.Api.DTOs.QuestionOption;

namespace HeimReport.Api.Validators.QuestionOptions;
public class QuestionOptionUpdateDtoValidator : AbstractValidator<QuestionOptionUpdateDto>
{
    public QuestionOptionUpdateDtoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required")
            .GreaterThan(0).WithMessage("Id must be a positive integer");
        RuleFor(x => x.QuestionId)
            .NotEmpty().WithMessage("QuestionId is required")
            .GreaterThan(0).WithMessage("QuestionId must be a positive integer");
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(200).WithMessage("Text must not exceed 200 characters");
        RuleFor(x => x.OrderIndex)
            .NotEmpty().WithMessage("OrderIndex is required")
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be a non-negative integer");
    }
}

