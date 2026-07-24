using FluentValidation;
using HeimReport.Api.DTOs.Positions;
using HeimReport.Api.Repositories.Positions;

namespace HeimReport.Api.Validators.Positions;

public class PositionCreateDtoValidator : AbstractValidator<PositionCreateDto>
{
    public PositionCreateDtoValidator(IPositionRepository repository)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(60).WithMessage("Title cannot exceed 60 characters")
            .MustAsync(async (title, ct) =>
                !await repository.ExistsByTitleAsync(title, excludeId: null, ct))
            .WithMessage("A position with this title already exists");

        RuleFor(x => x.CareerLevel)
            .IsInEnum().WithMessage("Invalid Career Level");
    }
}