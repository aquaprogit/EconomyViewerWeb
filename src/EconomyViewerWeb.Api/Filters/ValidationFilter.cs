using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;
using ApplicationValidationException =
    EconomyViewerWeb.Application.Exceptions.ValidationException;


namespace EconomyViewerWeb.Api.Filters;

public sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        var actionArguments = context.ActionArguments.Values;
        foreach (var argument in actionArguments)
        {
            if (argument is null)
            {
                continue;
            }

            var argumentType = argument.GetType();

            var validatorType = typeof(IValidator<>)
                .MakeGenericType(argumentType);

            var validator = context.HttpContext.RequestServices
                .GetService(validatorType);

            if (validator is not IValidator fluentValidator)
            {
                continue;
            }

            var validationContext =
                new ValidationContext<object>(argument);

            var validationResult =
                await fluentValidator.ValidateAsync(validationContext);

            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join(
                    "; ",
                    validationResult.Errors.Select(error => error.ErrorMessage));
                throw new ApplicationValidationException(errorMessage);
            }
        }
        await next();
    }
}
