using System.ComponentModel.DataAnnotations;

namespace HeimReport.Api.DTOs.Auth;

public record EmployeeRegistrationDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Required]
    [MaxLength(255)]
    public required string Username { get; init; }

    [Required]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public required string Password { get; init; }

    [Required]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    public required string ConfirmPassword { get; init; }
}