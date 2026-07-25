using FluentValidation;
using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Enums;

namespace HeimReport.Api.Validators.Employees;

public class EmployeeBulkTerminationDtoValidator : AbstractValidator<EmployeeBulkTerminationDto>
{
    public EmployeeBulkTerminationDtoValidator()
    {
        RuleFor(x => x.EmployeeIds)
            .NotEmpty().WithMessage("At least one employee must be selected");

        RuleFor(x => x.Status)
            .Must(s => s is EmployeeStatus.VoluntaryResignation
                or EmployeeStatus.InvoluntaryTermination
                or EmployeeStatus.ContractExpired)
            .WithMessage("Status must be a termination status (VoluntaryResignation, InvoluntaryTermination, or ContractExpired)");
    }
}