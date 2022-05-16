using FluentValidation.Results;

namespace Library.WebApi.Models.FailureResponses;

public record ValidationFailureResponseItem(string ProperyName = "", string ErrorMessage = "");
public class ValidationFailureResponse
{
    public IEnumerable<ValidationFailureResponseItem> Errors { get; init; } = Enumerable.Empty<ValidationFailureResponseItem>();
}

public static class ValidationFailureMapper
{
    public static ValidationFailureResponse ToResponse(this IEnumerable<ValidationFailure> failures)
    {
        return new ValidationFailureResponse
        {
            Errors = failures.Select(x => new ValidationFailureResponseItem(x.PropertyName, x.ErrorMessage))
        };
    }
}