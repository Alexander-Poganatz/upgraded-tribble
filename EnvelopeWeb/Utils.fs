﻿
module Utils

    open System.Security.Claims
    open Microsoft.AspNetCore.Http

    let Layout = "_Layout"
    let DefaultPaginationSize = 50

    let getUserIDFromClaims (principle:ClaimsPrincipal) =
        let claim = principle.Claims |> Seq.find (fun c -> c.Type = ClaimTypes.NameIdentifier)
        claim.Value |> System.Convert.ToInt32

    let doubleMoneyToCents (dbl:double) =
        let intPart = System.Convert.ToInt32 dbl

        let decimalPart = (dbl - System.Convert.ToDouble(intPart)) * 100.0 |> System.Convert.ToInt32
        (intPart * 100) + decimalPart

    let requestIsHTMX (httpContext:HttpContext) =
        let hasValue, valResult = httpContext.Request.Headers.TryGetValue "hx-request"
        hasValue && valResult.Equals("true")

    let htmxRequestLayout (httpContext:HttpContext) =
        if requestIsHTMX httpContext then null else Layout

    // Used for match expressions
    let (|IgnoreCaseEqual|_|) (s1:string) (s2:string) =
        match System.String.Compare(s1, s2, true) with
        | 0 -> Some()
        | _ -> None