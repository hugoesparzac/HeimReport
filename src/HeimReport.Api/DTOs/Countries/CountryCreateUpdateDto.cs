using System.ComponentModel.DataAnnotations;

namespace HeimReport.Api.DTOs.Countries;

public record CountryCreateUpdateDto(
    [Required(ErrorMessage =  "Name is required")]
    [MaxLength(60, ErrorMessage = "Name cannot exceed 60 characters")]
    string Name
);