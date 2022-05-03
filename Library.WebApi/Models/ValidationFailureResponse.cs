using FluentValidation.Results;

namespace Library.WebApi.Models;

public class ValidationFailureResponse
{
    public IEnumerable<string> Errors { get; init; } = Enumerable.Empty<string>();
}

public static class ValidationFailureMapper
{
    public static ValidationFailureResponse ToResponse(this IEnumerable<ValidationFailure> failure)
    {
        return new ValidationFailureResponse
        {
            Errors = failure.Select(x => x.ErrorMessage)
        };
    }
}