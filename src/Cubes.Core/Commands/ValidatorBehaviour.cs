using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Cubes.Core.Commands
{
    public class ValidatorBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        private readonly ILogger<ValidatorBehavior<TRequest, TResponse>> _logger;

        public ValidatorBehavior(
            IEnumerable<IValidator<TRequest>> validators,
            ILogger<ValidatorBehavior<TRequest, TResponse>> logger)
        {
            _validators = validators;
            _logger = logger;
        }

        // Based on http://softdevben.blogspot.com/2017/12/using-mediatr-pipeline-with-fluent.html
        public async Task<TResponse> Handle(TRequest request,
            CancellationToken cancellationToken,
            RequestHandlerDelegate<TResponse> next)
        {
            var context = new ValidationContext<TRequest>(request);
            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(r => r.Errors)
                .Where(fail => fail != null);

            if (failures.Any())
            {
                _logger.LogWarning("Validation for request {request} failed!", request);
                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
