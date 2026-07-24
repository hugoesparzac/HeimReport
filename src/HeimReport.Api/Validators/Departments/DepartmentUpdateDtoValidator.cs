using FluentValidation;
using HeimReport.Api.DTOs.Departments;
using HeimReport.Api.Repositories.Departments;

namespace HeimReport.Api.Validators.Departments;

public class DepartmentUpdateDtoValidator : AbstractValidator<DepartmentUpdateDto>
{
    public DepartmentUpdateDtoValidator(IDepartmentRepository repository)
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Invalid Department Id");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(60).WithMessage("Name cannot exceed 60 characters")
            .MustAsync(async (dto, name, ct) =>
                !await repository.ExistsByNameAsync(name, excludeId: dto.Id, ct))
            .WithMessage("A department with this name already exists");
    }
}
