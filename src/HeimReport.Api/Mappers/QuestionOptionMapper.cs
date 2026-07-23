using HeimReport.Api.DTOs.QuestionOption;
using HeimReport.Api.Entities;

namespace HeimReport.Api.Mappers;

public static class QuestionOptionMapper
{
    public static QuestionOption ToEntity(this QuestionOptionCreateDto dto)
    {
        return new QuestionOption
        {
            QuestionId = dto.QuestionId,
            Text = dto.Text,
            Value = dto.Value,
            OrderIndex = dto.OrderIndex
        };
    }

    public static QuestionOptionResponseDto ToResponseDto(this QuestionOption option)
    {
        return new QuestionOptionResponseDto
        {
            Id = option.Id,
            QuestionId = option.QuestionId,
            Text = option.Text,
            Value = option.Value,
            OrderIndex = option.OrderIndex
        };
    }

    public static void ApplyUpdate(this QuestionOption questionOption, QuestionOptionUpdateDto dto)
    {
        questionOption.Text = dto.Text;
        questionOption.OrderIndex = dto.OrderIndex;
        questionOption.Value = dto.Value;
    }
}