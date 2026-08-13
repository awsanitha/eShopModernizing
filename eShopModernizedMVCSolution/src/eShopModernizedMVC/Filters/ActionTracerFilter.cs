using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace eShopModernizedMVC.Filters
{
    public class ActionTracerFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            Trace.TraceInformation($"Received request for action {context.ActionDescriptor.DisplayName}.");
            base.OnActionExecuting(context);
        }
    }
}
