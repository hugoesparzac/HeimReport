using FluentValidation;
using HeimReport.Api.DTOs.Employees;

namespace HeimReport.Api.Validators.Employees;

public class EmployeeCreateUpdateDtoValidator : AbstractValidator<EmployeeCreateUpdateDto>
{
    public EmployeeCreateUpdateDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First Name is required")
            .MaximumLength(50);
        
        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last Name is required")
            .MaximumLength(100);
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(100);
        
        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("National ID is required")
            .MaximumLength(50);

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire Date is required");

        RuleFor(x => x.ContractType)
            .NotEmpty().WithMessage("Contract Type is required");
        
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required");
        
        RuleFor(x => x.CountryId)
            .NotEmpty().WithMessage("Country ID is required");
        
        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department ID is required");
        
        RuleFor(x => x.PositionId)
            .NotEmpty().WithMessage("Position ID is required");
    }
}