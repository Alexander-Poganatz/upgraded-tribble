module CustomMiddleware

open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Primitives
open System.Collections.Generic

let private contentSecurityPolicy =
#if DEBUG
    StringValues("default-src 'self'; connect-src http://localhost:* ws://localhost:* wss://localhost:*;")
#else
    StringValues("default-src 'self';")
#endif

// copied from owasp.org
let private permissionPolicy =
    StringValues("accelerometer=(),ambient-light-sensor=(),autoplay=(),battery=(),camera=(),display-capture=(),document-domain=(),encrypted-media=(),fullscreen=(),gamepad=(),geolocation=(),gyroscope=(),layout-animations=(),legacy-image-formats=(),magnetometer=(),microphone=(),midi=(),oversized-images=(),payment=(),picture-in-picture=(),publickey-credentials-get=(),speaker-selection=(),sync-xhr=(),unoptimized-images=(),unsized-media=(),usb=(),screen-wake-lock=(),web-share=(),xr-spatial-tracking=()")

let deny = StringValues("DENY")

let nosniff = StringValues("nosniff")


let private permissionPolicyPair = KeyValuePair.Create("Permissions-Policy", permissionPolicy)
// Adds headers for all web pages
let addHeaderMiddleWare (context: HttpContext) (next: RequestDelegate) =
    context.Response.Headers.ContentSecurityPolicy <- contentSecurityPolicy;
    context.Response.Headers.Add(permissionPolicyPair);
    context.Response.Headers.XFrameOptions <- deny;
    context.Response.Headers.XContentTypeOptions <- nosniff;

    next.Invoke(context)