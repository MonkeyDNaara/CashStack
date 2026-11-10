using CashStack.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace CashStack.Api.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class ValidateDateRangeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        DateRangeQuery? range = context.ActionArguments.Values.OfType<DateRangeQuery>().FirstOrDefault();
        if (range is null || range.Start <= range.End)
        {
            return;
        }

        context.ModelState.AddModelError(
            "from",
            $"'from' ({range.Start:yyyy-MM-dd}) must not be after 'to' ({range.End:yyyy-MM-dd}).");

        // Same format as the automatic 400 responses of [ApiController] (incl. traceId)
        var factory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        ValidationProblemDetails problem = factory.CreateValidationProblemDetails(context.HttpContext, context.ModelState);

        // Setting Result short-circuits the pipeline: the action is not executed
        context.Result = new BadRequestObjectResult(problem);
    }
}
