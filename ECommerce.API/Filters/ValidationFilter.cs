using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;
using ECommerce.Common.Result;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerce.API.Filters
{
    /// <summary>
    /// ValidationFilter: Intercepts requests to validate action parameters using FluentValidation.
    /// Returns a standardized Result.Failure response with a 400 Bad Request status code.
    /// </summary>
    public class ValidationFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidationFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null) continue;

                // Resolve IValidator<T> where T is the argument's type
                var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
                var validator = _serviceProvider.GetService(validatorType) as IValidator;

                if (validator != null)
                {
                    var validationContext = new ValidationContext<object>(argument);
                    var validationResult = await validator.ValidateAsync(validationContext);

                    if (!validationResult.IsValid)
                    {
                        var errors = validationResult.Errors
                            .Select(e => e.ErrorMessage)
                            .ToList();

                        var result = Result.Failure("Validation failed.", errors);
                        context.Result = new BadRequestObjectResult(result);
                        return;
                    }
                }
            }

            await next();
        }
    }
}
