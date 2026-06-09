using FluentValidation;
using MediatR;

namespace Oms.Application.Common.Behaviours;

// The generic constraints ensure this behavior intercepts any MediatR Request that returns a Response
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    // Inject all validators registered in DI that match the incoming command/request type
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken cancellationToken)
    {
        // Only run validation if there are validators registered for this type of command
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);

            // Execute all validators concurrently
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            // Gather all errors that were returned
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            // If we have any failures, halt the pipeline and throw a ValidationException
            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
        }
        
        // If there are no failures (or no validators), call next() to run the next behavior or the main Handler
        return await next();
    }
}