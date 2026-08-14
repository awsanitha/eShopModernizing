using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.Filters;

namespace eShopModernizedMVC.Filters
{
    public class ActionTracerFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Trace.TraceInformation($"Received request for action {filterContext.ActionDescriptor.DisplayName}.");
            base.OnActionExecuting(filterContext);
        }
    }
}
