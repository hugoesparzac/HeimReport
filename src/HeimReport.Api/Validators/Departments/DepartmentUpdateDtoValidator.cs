using FluentValidation;
using HeimReport.Api.DTOs.Departments;
using HeimReport.Api.Repositories.Departments;

namespace HeimReport.Api.Validators.Departments;

public class DepartmentUpdateDtoValidator : AbstractValidator<DepartmentUpdateDto>
{
    public DepartmentUpdateDtoValidator(IDepartmentRepository repository)
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters")
            .MustAsync(async (_, name, context, ct) =>
                !await repository.ExistsByNameAsync(name, GetCurrentId(context), ct))
            .WithMessage("A department with this name already exists");
    }

    private static int GetCurrentId(ValidationContext<DepartmentUpdateDto> context)
    {
        if (context.RootContextData.TryGetValue("DepartmentId", out var value) && value is int id)
        {
            return id;
        }

        throw new InvalidOperationException(
            "DepartmentId must be set in RootContextData before validating DepartmentUpdateDto.");
    }
}