using FluentValidation;

namespace HeimReport.Api.Extensions;

public static class ValidatorExtensions
{
    public static async Task ValidateOrThrowAsync<T>(
        this IValidator<T> validator, T instance, CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }

    public static async Task ValidateOrThrowAsync<T>(
        this IValidator<T> validator, IValidationContext context, CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(context, cancellationToken);
        if (!result.IsValid)
        {
            throw new ValidationException(result.Errors);
        }
    }
}