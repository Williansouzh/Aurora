using Aurora.Application.Abstractions.Persistence;
using MediatR;

namespace Aurora.Application.Behaviors;

public class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!typeof(TRequest).Name.EndsWith("Command", StringComparison.Ordinal))
        {
            return await next();
        }

        TResponse? response = default;
        await unitOfWork.ExecuteInTransactionAsync(async _ => response = await next(), cancellationToken);
        return response!;
    }
}
