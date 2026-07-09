using FluentValidation;
using HeimReport.Api.DTOs.Employees;
namespace HeimReport.Api.Validators.Employees;

public class EmployeeCreateDtoValidator : AbstractValidator<EmployeeCreateDto>
{
    public EmployeeCreateDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First Name is required")
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name is required")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100);

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("National ID is required")
            .MaximumLength(50);

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire Date is required")
            .LessThanOrEqualTo(_ => DateTime.UtcNow).WithMessage("Hire Date cannot be in the future");

        RuleFor(x => x.ContractType)
            .IsInEnum().WithMessage("Invalid Contract Type");

        RuleFor(x => x.ContractEndDate)
            .GreaterThan(x => x.HireDate).WithMessage("Contract End Date must be after Hire Date")
            .When(x => x.ContractEndDate.HasValue);

        RuleFor(x => x.CountryId)
            .GreaterThan(0).WithMessage("Country ID is required");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Department ID is required");

        RuleFor(x => x.PositionId)
            .GreaterThan(0).WithMessage("Position ID is required");

        RuleFor(x => x.ManagerId)
            .GreaterThan(0).WithMessage("Manager ID must be a valid ID")
            .When(x => x.ManagerId.HasValue);
    }
}