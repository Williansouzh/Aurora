using DomainValidationException = Aurora.Domain.Exceptions.ValidationException;
using FluentValidation;
using MediatR;

namespace Aurora.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validatorsList = validators.ToList();
        if (validatorsList.Count == 0)
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(
            validatorsList.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).Where(e => e is not null).ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var errors = failures
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).Distinct().ToArray());

        throw new DomainValidationException("Validation failed", errors);
    }
}
