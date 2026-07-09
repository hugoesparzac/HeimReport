using FluentValidation;
using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Enums;
namespace HeimReport.Api.Validators.Employees;

public class EmployeeUpdateDtoValidator : AbstractValidator<EmployeeUpdateDto>
{
    public EmployeeUpdateDtoValidator()
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

        RuleFor(x => x.ContractType)
            .IsInEnum().WithMessage("Invalid Contract Type");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid Status");

        RuleFor(x => x.TerminationDate)
            .NotEmpty().WithMessage("Termination Date is required when status indicates the employee has left")
            .When(x => x.Status is EmployeeStatus.VoluntaryResignation
                        or EmployeeStatus.InvoluntaryTermination
                        or EmployeeStatus.ContractExpired);

        RuleFor(x => x.TerminationDate)
            .Null().WithMessage("Termination Date must be empty for active employees")
            .When(x => x.Status == EmployeeStatus.Active);

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