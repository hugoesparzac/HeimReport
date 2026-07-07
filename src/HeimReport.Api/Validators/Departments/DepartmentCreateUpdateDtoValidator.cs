using FluentValidation;
using HeimReport.Api.DTOs.Departments;

namespace HeimReport.Api.Validators.Departments;

public class DepartmentCreateUpdateDtoValidator : AbstractValidator<DepartmentCreateUpdateDto>
{
    public DepartmentCreateUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");
    }
}