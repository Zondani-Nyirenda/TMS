using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TMS.API.Filters;

/// <summary>
/// Global action filter that short-circuits with 400 if ModelState is invalid.
/// Works alongside FluentValidation's automatic model-state integration.
/// </summary>
public class ValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            context.Result = new BadRequestObjectResult(new
            {
                success = false,
                message = "Validation failed.",
                errors,
                statusCode = 400
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}