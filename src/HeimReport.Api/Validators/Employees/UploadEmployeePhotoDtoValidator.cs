using FluentValidation;
using HeimReport.Api.DTOs.Employees;

namespace HeimReport.Api.Validators.Employees;

public class UploadEmployeePhotoDtoValidator : AbstractValidator<UploadEmployeePhotoDto>
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public UploadEmployeePhotoDtoValidator()
    {
        RuleFor(x => x.Photo)
            .NotNull().WithMessage("A photo file is required")
            .Must(f => f.Length > 0).WithMessage("The uploaded file is empty")
            .Must(f => f.Length <= MaxFileSizeBytes).WithMessage("The photo must not exceed 5 MB")
            .Must(f => AllowedContentTypes.Contains(f.ContentType))
                .WithMessage("Only JPEG, PNG, or WEBP images are allowed");
    }
}