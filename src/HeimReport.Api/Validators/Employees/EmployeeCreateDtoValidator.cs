using FluentValidation;
using HeimReport.Api.DTOs.Employees;
using HeimReport.Api.Repositories.Countries;
using HeimReport.Api.Repositories.Departments;
using HeimReport.Api.Repositories.Employees;
using HeimReport.Api.Repositories.Positions;

namespace HeimReport.Api.Validators.Employees;

public class EmployeeCreateDtoValidator : AbstractValidator<EmployeeCreateDto>
{
    public EmployeeCreateDtoValidator(
        IEmployeeRepository employeeRepository,
        ICountryRepository countryRepository,
        IDepartmentRepository departmentRepository,
        IPositionRepository positionRepository)
    {
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
            .MustAsync(async (email, ct) =>
                !await employeeRepository.ExistsByNormalizedEmailAsync(email.ToUpperInvariant(), excludeId: null, ct))
            .WithMessage("An employee with this email already exists");

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("National ID is required")
            .MaximumLength(50).WithMessage("National ID cannot exceed 50 characters");

        RuleFor(x => x)
            .MustAsync(async (dto, ct) =>
                !await employeeRepository.ExistsByNationalIdAndCountryAsync(dto.NationalId, dto.CountryId, excludeId: null, ct))
            .WithMessage("An employee with this National ID already exists in this country")
            .WithName("NationalId")
            .When(x => !string.IsNullOrWhiteSpace(x.NationalId) && x.CountryId > 0);

        RuleFor(x => x.BirthDate)
            .NotEmpty().WithMessage("Birth Date is required")
            .LessThan(DateTime.UtcNow.AddYears(-15)).WithMessage("Employee must be at least 15 years old");

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire Date is required");

        RuleFor(x => x.ContractType)
            .IsInEnum().WithMessage("Invalid Contract Type");

        RuleFor(x => x.ContractEndDate)
            .GreaterThan(x => x.HireDate).WithMessage("Contract End Date must be after Hire Date")
            .When(x => x.ContractEndDate.HasValue);

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
            .MustAsync(async (id, ct) => await employeeRepository.ExistsActiveAsync(id!.Value, ct))
            .WithMessage("Manager does not exist or is not active")
            .When(x => x.ManagerId.HasValue);
    }
}