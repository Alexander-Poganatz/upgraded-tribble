using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace EnvelopeASP
{
    public class HTMXDefaultLayoutFilterAttribute : ActionFilterAttribute
    {
        public HTMXDefaultLayoutFilterAttribute() { }

        public override void OnResultExecuting(ResultExecutingContext context)
        {
            var layoutToUse = Utils.HTMXRequestLayout(context.HttpContext);
            var controller = (Controller)context.Controller;
            // I only expect this attribute to be used on controllers so I'm not doing any safety checks.
            controller.ViewData[Utils.LAYOUT] = layoutToUse;

            base.OnResultExecuting(context);
        }
    }
}
