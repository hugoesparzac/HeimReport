using System.ComponentModel.DataAnnotations;
using HeimReport.Api.Enums;

namespace HeimReport.Api.DTOs.Positions;

public record PositionCreateUpdateDto(string Title, CareerLevel CareerLevel, bool IsCritical);