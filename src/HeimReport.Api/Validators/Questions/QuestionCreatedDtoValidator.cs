using FluentValidation;
using HeimReport.Api.DTOs.Questions;

namespace HeimReport.Api.Validators.Questions;
public class QuestionCreatedDtoValidator : AbstractValidator<QuestionCreateDto>
{
    public QuestionCreatedDtoValidator()
    {
        RuleFor(x => x.SurveyTemplateId)
            .NotEmpty().WithMessage("SurveyTemplateId is required")
            .GreaterThan(0).WithMessage("SurveyTemplateId must be a positive integer");
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(500).WithMessage("Text must not exceed 500 characters");
        RuleFor(x => x.QuestionType)
            .IsInEnum().WithMessage("QuestionType must be a valid enum value");
        RuleFor(x => x.OrderIndex)
            .NotEmpty().WithMessage("OrderIndex is required")
            .GreaterThanOrEqualTo(0).WithMessage("OrderIndex must be a non-negative integer");
    }
}

