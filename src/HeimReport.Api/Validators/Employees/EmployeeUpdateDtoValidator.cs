using FluentValidation;
using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Enums;
using HeimReport.Api.Repositories.Countries;
using HeimReport.Api.Repositories.Departments;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Repositories.Positions;

namespace HeimReport.Api.Validators.Employees;

public class EmployeeUpdateDtoValidator : AbstractValidator<EmployeeUpdateDto>
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeUpdateDtoValidator(
        IEmployeeRepository employeeRepository,
        ICountryRepository countryRepository,
        IDepartmentRepository departmentRepository,
        IPositionRepository positionRepository)
    {
        _employeeRepository = employeeRepository;

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First Name is required")
            .MaximumLength(50).WithMessage("First Name cannot exceed 50 characters");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name is required")
            .MaximumLength(100).WithMessage("Last Name cannot exceed 100 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100).WithMessage("Email cannot exceed 100 characters")
            .MustAsync(async (_, email, context, ct) =>
                !await employeeRepository.ExistsByNormalizedEmailAsync(
                    email.ToUpperInvariant(), GetCurrentEmployeeId(context), ct))
            .WithMessage("Another employee already uses this email");

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("National ID is required")
            .MaximumLength(50).WithMessage("National ID cannot exceed 50 characters");

        RuleFor(x => x)
            .MustAsync(async (dto, _, context, ct) =>
                !await employeeRepository.ExistsByNationalIdAndCountryAsync(
                    dto.NationalId, dto.CountryId, GetCurrentEmployeeId(context), ct))
            .WithMessage("Another employee already uses this National ID in this country")
            .WithName("NationalId")
            .When(x => !string.IsNullOrWhiteSpace(x.NationalId) && x.CountryId > 0);

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth Date is required")
            .LessThan(DateTime.UtcNow.AddYears(-15)).WithMessage("Employee must be at least 15 years old");

        RuleFor(x => x.ContractType)
            .IsInEnum().WithMessage("Invalid Contract Type");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Invalid Status");

        RuleFor(x => x.TerminationDate)
            .NotEmpty().WithMessage("Termination Date is required when status indicates the employee has left")
            .When(x => x.Status is EmployeeStatus.VoluntaryResignation or EmployeeStatus.InvoluntaryTermination or EmployeeStatus.ContractExpired);

        RuleFor(x => x.TerminationDate)
            .Null().WithMessage("Termination Date must be empty for active employees")
            .When(x => x.Status == EmployeeStatus.Active);

        RuleFor(x => x.CurrentSalary)
            .GreaterThanOrEqualTo(0).WithMessage("Salary cannot be negative");

        RuleFor(x => x.CountryId)
            .GreaterThan(0).WithMessage("Country ID is required")
            .MustAsync(async (id, ct) => await countryRepository.ExistsActiveAsync(id, ct))
            .WithMessage("Country does not exist or is not active");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0).WithMessage("Department ID is required")
            .MustAsync(async (id, ct) => await departmentRepository.ExistsActiveAsync(id, ct))
            .WithMessage("Department does not exist or is not active");

        RuleFor(x => x.PositionId)
            .GreaterThan(0).WithMessage("Position ID is required")
            .MustAsync(async (id, ct) => await positionRepository.ExistsActiveAsync(id, ct))
            .WithMessage("Position does not exist or is not active");

        RuleFor(x => x.ManagerId)
            .GreaterThan(0).WithMessage("Manager ID must be a valid ID")
            .Must((_, managerId, context) => managerId != GetCurrentEmployeeId(context))
            .WithMessage("An employee cannot be their own manager")
            .MustAsync(async (id, ct) => await employeeRepository.ExistsActiveAsync(id!.Value, ct))
            .WithMessage("Manager does not exist or is not active")
            .MustAsync(async (_, managerId, context, ct) =>
                !await WouldCreateCycleAsync(GetCurrentEmployeeId(context), managerId!.Value, ct))
            .WithMessage("This assignment would create a circular reporting relationship")
            .When(x => x.ManagerId.HasValue);

        RuleFor(x => x.ChangeReason)
            .IsInEnum().WithMessage("Invalid change reason")
            .NotEqual(JobChangeReason.Termination)
                .WithMessage("Termination reason is set automatically and cannot be submitted manually")
            .NotEqual(JobChangeReason.Reactivation)
                .WithMessage("Reactivation reason is set automatically and cannot be submitted manually")
            .When(x => x.ChangeReason.HasValue);

        RuleFor(x => x.ChangeReason)
            .NotNull().WithMessage("A change reason is required when Department, Position, Manager or Salary changes")
            .When((dto, context) => RequiresChangeReason(dto, context), ApplyConditionTo.CurrentValidator);

        RuleFor(x => x.OtherReasonDetail)
            .NotEmpty().WithMessage("Please specify the reason when selecting 'Other'")
            .MaximumLength(255).WithMessage("Reason detail cannot exceed 255 characters")
            .When(x => x.ChangeReason == JobChangeReason.Other);

        RuleFor(x => x.OtherReasonDetail)
            .Empty().WithMessage("Reason detail should only be provided when ChangeReason is 'Other'")
            .When(x => x.ChangeReason.HasValue && x.ChangeReason != JobChangeReason.Other);
    }

    private static int GetCurrentEmployeeId(ValidationContext<EmployeeUpdateDto> context)
    {
        if (context.RootContextData.TryGetValue("EmployeeId", out var value) && value is int id)
        {
            return id;
        }

        throw new InvalidOperationException(
            "EmployeeId must be set in RootContextData before validating EmployeeUpdateDto.");
    }

    private static EmployeeSnapshot? GetSnapshot(ValidationContext<EmployeeUpdateDto> context)
    {
        return context.RootContextData.TryGetValue("CurrentSnapshot", out var value) && value is EmployeeSnapshot snapshot
            ? snapshot
            : null;
    }

    private static bool RequiresChangeReason(EmployeeUpdateDto dto, ValidationContext<EmployeeUpdateDto> context)
    {
        var snapshot = GetSnapshot(context);
        if (snapshot is null)
        {
            return false;
        }

        var wasOnLeave = snapshot.Status is EmployeeStatus.VoluntaryResignation
            or EmployeeStatus.InvoluntaryTermination
            or EmployeeStatus.ContractExpired;

        var isNowOnLeave = snapshot.Status == EmployeeStatus.Active
            && dto.Status is EmployeeStatus.VoluntaryResignation
                or EmployeeStatus.InvoluntaryTermination
                or EmployeeStatus.ContractExpired;

        var isReactivation = wasOnLeave && dto.Status == EmployeeStatus.Active;

        if (isNowOnLeave || isReactivation)
        {
            return false;
        }

        return dto.DepartmentId != snapshot.DepartmentId
            || dto.PositionId != snapshot.PositionId
            || dto.ManagerId != snapshot.ManagerId
            || dto.CurrentSalary != snapshot.CurrentSalary;
    }

    private async Task<bool> WouldCreateCycleAsync(int employeeId, int proposedManagerId, CancellationToken ct)
    {
        var chain = await _employeeRepository.GetManagerChainAsync(proposedManagerId, ct);
        return chain.Contains(employeeId);
    }
}