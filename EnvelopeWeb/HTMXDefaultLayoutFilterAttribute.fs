
namespace EnvelopeWeb

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.Filters

type HTMXDefaultLayoutFilterAttribute() =
    inherit ActionFilterAttribute()

    override this.OnResultExecuting (context:ResultExecutingContext) =
        
            let layoutToUse = Utils.htmxRequestLayout context.HttpContext
            let controller = context.Controller :?> Controller
            // I only expect this attribute to be used on controllers so I'm not doing any safety checks.
            controller.ViewData[Utils.Layout] <- layoutToUse;

            base.OnResultExecuting(context);
        
