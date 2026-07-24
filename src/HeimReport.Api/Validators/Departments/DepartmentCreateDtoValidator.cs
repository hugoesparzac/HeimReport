using FluentValidation;
using HeimReport.Api.DTOs.Departments;
using HeimReport.Api.Repositories.Departments;

namespace HeimReport.Api.Validators.Departments;

public class DepartmentCreateDtoValidator : AbstractValidator<DepartmentCreateDto>
{
    public DepartmentCreateDtoValidator(IDepartmentRepository repository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
            .MustAsync(async (name, ct) =>
                !await repository.ExistsByNameAsync(name, excludeId: null, ct))
            .WithMessage("A department with this name already exists");
    }
}