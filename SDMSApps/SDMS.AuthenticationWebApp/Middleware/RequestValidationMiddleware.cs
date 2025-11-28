using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SDMS.AuthenticationWebApp.Models.Common;

namespace SDMS.AuthenticationWebApp.Middleware;

/// <summary>
/// Action filter for automatic request validation
/// </summary>
public class ValidateRequestAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var correlationId = context.HttpContext.Items["CorrelationId"]?.ToString();
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                .ToList();

            var response = ApiResponse.ErrorResponse(
                message: "Validation failed",
                errors: errors,
                correlationId: correlationId);

            context.Result = new BadRequestObjectResult(response);
        }
    }
}

