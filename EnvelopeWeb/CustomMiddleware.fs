module CustomMiddleware

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives

let private contentSecurityPolicy =
#if DEBUG
    StringValues("default-src 'self'; connect-src http://localhost:* ws://localhost:* wss://localhost:*;")
#else
    StringValues("default-src 'self';")
#endif

// Adds headers for all web pages
let addHeaderMiddleWare (context: HttpContext) (next: RequestDelegate) =
    context.Response.Headers.ContentSecurityPolicy <- contentSecurityPolicy;

    next.Invoke(context)