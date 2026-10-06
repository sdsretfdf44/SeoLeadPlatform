using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SeoLeadPlatform.Api.Middleware;

public class FluentValidationActionFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _provider;

    public FluentValidationActionFilter(IServiceProvider provider) => _provider = provider;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var kvp in context.ActionArguments)
        {
            if (kvp.Value is null) continue;
            var argType = kvp.Value.GetType();
            var validatorType = typeof(IValidator<>).MakeGenericType(argType);
            var validator = _provider.GetService(validatorType) as IValidator;
            if (validator is null) continue;

            var validationContext = new ValidationContext<object>(kvp.Value);
            var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);
            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors)
                {
                    Title = "Validation failed.",
                    Status = 400
                });
                return;
            }
        }
        await next();
    }
}
