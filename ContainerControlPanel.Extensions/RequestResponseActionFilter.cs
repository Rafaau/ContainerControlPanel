using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace ContainerControlPanel.Extensions;

public class RequestResponseActionFilter : IActionFilter
{
    private readonly ILogger<RequestResponseActionFilter> _logger;

    public RequestResponseActionFilter(ILogger<RequestResponseActionFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        _logger?.LogRequest(context.HttpContext.Request);
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        object body = null;

        if (context.Result is ObjectResult objectResult)
        {
            body = objectResult.Value;
        }
        else if (context.Result is ContentResult contentResult)
        {
            body = contentResult.Content;
        }
        else if (context.Result is JsonResult jsonResult)
        {
            body = jsonResult.Value;
        }
        else if (context.Result is StatusCodeResult statusCodeResult)
        {
            body = statusCodeResult.StatusCode;
        }

        _logger?.LogResponse(body);
    }
}
