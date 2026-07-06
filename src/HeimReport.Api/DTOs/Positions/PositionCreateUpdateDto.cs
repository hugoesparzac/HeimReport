using System.ComponentModel.DataAnnotations;
using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Positions;

public record PositionCreateUpdateDto(
    [Required(ErrorMessage =  "Title is required")]
    [MaxLength(100, ErrorMessage ="Title cannot exceed 100 characters")]
    string Title,
    [Required(ErrorMessage =  "CareerLevel is required")]
    CareerLevel CareerLevel,
    [Required(ErrorMessage =  "IsCritical flag is required")]
    bool IsCritical
);