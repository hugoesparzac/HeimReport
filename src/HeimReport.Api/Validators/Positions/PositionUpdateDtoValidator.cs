using FluentValidation;
using HeimReport.Api.DTOs.Positions;
using HeimReport.Api.Repositories.Positions;

namespace HeimReport.Api.Validators.Positions;

public class PositionUpdateDtoValidator : AbstractValidator<PositionUpdateDto>
{
    public PositionUpdateDtoValidator(IPositionRepository repository)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid Position Id");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(60).WithMessage("Title cannot exceed 60 characters")
            .MustAsync(async (dto, title, ct) =>
                !await repository.ExistsByTitleAsync(title, excludeId: dto.Id, ct))
            .WithMessage("A position with this title already exists");

        RuleFor(x => x.CareerLevel)
            .IsInEnum().WithMessage("Invalid Career Level");
    }
}