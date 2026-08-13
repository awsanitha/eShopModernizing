using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace eShopModernizedMVC.Filters
{
    public class ActionTracerFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            Trace.TraceInformation($"Received request for action {context.ActionDescriptor.DisplayName}.");
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}
