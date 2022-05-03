using FluentValidation;
using Library.WebApi.Models;

namespace Library.WebApi.Filters;

public class ValidationFilter<T> : IRouteHandlerFilter where T : class
{
    private readonly IValidator<T> _validator;

    public ValidationFilter(IValidator<T> validator)
    {
        _validator = validator;
    }

    public async ValueTask<object?> InvokeAsync(RouteHandlerInvocationContext context, RouteHandlerFilterDelegate next)
        => context.Parameters.SingleOrDefault(x => x?.GetType() == typeof(T)) switch
        {
            T validatable => await _validator.ValidateAsync(validatable) switch
            {
                { IsValid: true } => await next(context),
                var result        => Results.BadRequest(result.Errors.ToResponse())
            },
            _             => Results.BadRequest()
        };
}