module HTMLViewHandles

open Models
open Giraffe
open Microsoft.AspNetCore.Http
open DbConnection
open Microsoft.Extensions.Logging
open System.Security.Claims
open Microsoft.AspNetCore.Authentication

let private isLoggedIn (ctx: HttpContext) = isNotNull ctx.User.Identity && ctx.User.Identity.IsAuthenticated

let private redirectUrlParamName = "redirectUrl"

// https://github.com/giraffe-fsharp/Giraffe/discussions/458
let private createAntiForgeryTokenNode (ctx: HttpContext) =
    let antiforgery = ctx.RequestServices.GetService(typeof<Microsoft.AspNetCore.Antiforgery.IAntiforgery>) :?> Microsoft.AspNetCore.Antiforgery.IAntiforgery

    let token_set = antiforgery.GetAndStoreTokens(ctx)

    Giraffe.ViewEngine.HtmlElements.input [ 
        Giraffe.ViewEngine.Attributes._name token_set.FormFieldName; 
        Giraffe.ViewEngine.Attributes._value token_set.RequestToken;
        Giraffe.ViewEngine.Attributes._type "hidden" ]

let homeViewHandle (next : HttpFunc) (ctx : HttpContext) =
        let isAuthenticated = isNotNull ctx.User.Identity && ctx.User.Identity.IsAuthenticated
        (htmlView (HTMLViews.homeView isAuthenticated)) next ctx

let emptyLoginErrors = { EmailError = ""; PasswordError = ""; ConfirmPasswordError = ""; }

let loginGetHandle (next : HttpFunc) (ctx : HttpContext) =
    let antiForgeryTag = createAntiForgeryTokenNode ctx
    (htmlView (HTMLViews.loginView antiForgeryTag emptyLoginErrors { Password = ""; Email = ""; })) next ctx

let signUpGetHandle (next : HttpFunc) (ctx : HttpContext) =
    let antiForgeryTag = createAntiForgeryTokenNode ctx
    (htmlView (HTMLViews.signupView antiForgeryTag emptyLoginErrors { Password = ""; Email = ""; ConfirmPassword = "" })) next ctx

let resetPasswordGetHandle (next : HttpFunc) (ctx : HttpContext) =
    let antiForgeryTag = createAntiForgeryTokenNode ctx
    (htmlView (HTMLViews.resetPasswordView antiForgeryTag emptyLoginErrors { Password = ""; Email = ""; ConfirmPassword = "" })) next ctx

let validateAntiForgeryToken (next : HttpFunc) (ctx : HttpContext) =
    task {
        let antiforgery = ctx.RequestServices.GetService(typeof<Microsoft.AspNetCore.Antiforgery.IAntiforgery>) :?> Microsoft.AspNetCore.Antiforgery.IAntiforgery
        let! isValid = antiforgery.IsRequestValidAsync(ctx)
        if isValid then
            return! next ctx
        else
            return! setStatusCode 400 earlyReturn ctx
    }

let requireLogin (next : HttpFunc) (ctx : HttpContext) =
    match isLoggedIn ctx with
    | true -> next ctx
    | false ->
        let accountURL = "/Account/Login?" + redirectUrlParamName + "=" + ctx.Request.Path
        (redirectTo false accountURL) earlyReturn ctx

let requireSignOut (next : HttpFunc) (ctx : HttpContext) =
    match isLoggedIn ctx with
    | true -> (redirectTo false "/") earlyReturn ctx
    | false -> next ctx

let private attemptLogin (dbConnection: DbConnectionGetter) (model: Login) (ctx: HttpContext) =
    task {
        let errorModel = { 
                        EmailError = if System.String.IsNullOrWhiteSpace(model.Email) then "Name is required" else System.String.Empty;
                        PasswordError = if System.String.IsNullOrWhiteSpace(model.Password) then 
                                                "Password is required" 
                                            elif model.Password.Length > 1024 then 
                                                "Password is a bit big, rejected"
                                            else
                                                System.String.Empty;
                        ConfirmPasswordError = System.String.Empty; 
                        
                    }
        match errorModel with
        | { EmailError = ""; PasswordError = ""; ConfirmPasswordError = "" } ->
            let email = model.Email.Trim()
            let! dbUser = Procedures.Sel_UserByEmail dbConnection email
            match dbUser with
            | None -> return Error { errorModel with EmailError = "Invalid username or password" }
            | Some dbUser ->
                    let isValid = System.DateTime.UtcNow > dbUser.LockoutExpiry && (Password.verify dbUser.PasswordConfig model.Password dbUser.PasswordHash)
                            
                    let! r = Procedures.Upd_User_Login dbConnection dbUser.Id isValid

                    if isValid then 
                        let idClaim = new Claim(ClaimTypes.NameIdentifier, dbUser.Id.ToString())
                        let nameClaim = new Claim(ClaimTypes.Name, email)
                        let claims = [| idClaim; nameClaim; |]
                        let claimsIdentity = new ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
                        let principle = new ClaimsPrincipal(claimsIdentity)

                        let! r = ctx.SignInAsync(principle)

                        return Ok ()
                    else
                        return Error { errorModel with EmailError = "Invalid username or password" }
                            
        | _ ->
                return Error errorModel
    }

let loginPostHandle (next : HttpFunc) (ctx : HttpContext) =
    task {
        let! modelResult = ctx.TryBindFormAsync<Login>()
        match modelResult with
        | Ok model -> 
                        let dbConnection = ctx.GetService<DbConnectionGetter>()
                        let! loginResult = attemptLogin dbConnection model ctx
                        match loginResult with
                        | Error e -> 
                                let verificationTag = createAntiForgeryTokenNode ctx
                                return! (htmlView (HTMLViews.loginView verificationTag e model)) next ctx
                        | Ok _ ->
                                let redirectUrl = if ctx.Request.Query.ContainsKey redirectUrlParamName then (ctx.Request.Query.Item redirectUrlParamName)[0] else "/"
                                return! (redirectTo false redirectUrl) next ctx
        | Error e ->
            return! setStatusCode 400 next ctx
    }

let private attemptSignup (dbConnection: DbConnectionGetter) (model: SignUp) =
    task {
        let errorModel = { 
                        EmailError = if System.String.IsNullOrWhiteSpace(model.Email) then "Name is required" else System.String.Empty;
                        PasswordError = if System.String.IsNullOrWhiteSpace(model.Password) then 
                                                "Password is required" 
                                            elif model.Password.Length > 1024 then 
                                                "Password is a bit big, rejected"
                                            else
                                                System.String.Empty;
                        ConfirmPasswordError = if model.ConfirmPassword <> model.Password then "Passwords do not match" else System.String.Empty; 
                        
                    }
        match errorModel with
        | { EmailError = ""; PasswordError = ""; ConfirmPasswordError = "" } ->
            let email = model.Email.Trim()
            let passwordConfig = Password.makeDefaultPasswordConfig()
            let hash = Password.generateHash passwordConfig model.Password

            let! r = Procedures.InsertUser dbConnection email hash passwordConfig

            return Ok ()
                            
        | _ ->
                return Error errorModel
    }

let signUpPostHandle (next : HttpFunc) (ctx : HttpContext) =
    task {
        let! modelResult = ctx.TryBindFormAsync<SignUp>()
        match modelResult with
        | Ok model -> 
                        let dbConnection = ctx.GetService<DbConnectionGetter>()
                        let! signUpResult = attemptSignup dbConnection model
                        match signUpResult with
                        | Error e -> 
                                let verificationTag = createAntiForgeryTokenNode ctx
                                return! (htmlView (HTMLViews.signupView verificationTag e model)) next ctx
                        | Ok _ -> return! (redirectTo false "/Account/Login") next ctx
        | Error e ->
            return! setStatusCode 400 next ctx
    }
    
let attemptPasswordReset (dbConnection: DbConnectionGetter) (model: SignUp) (ctx: HttpContext) =
   task {
        let errorModel = { 
                        EmailError = if System.String.IsNullOrWhiteSpace(model.Email) then "Current password is required" else System.String.Empty;
                        PasswordError = if System.String.IsNullOrWhiteSpace(model.Password) then 
                                                "New Password is required" 
                                            elif model.Password.Length > 1024 then 
                                                "New Password is a bit big, rejected"
                                            else
                                                System.String.Empty;
                        ConfirmPasswordError = if model.ConfirmPassword <> model.Password then "Passwords do not match" else System.String.Empty;
                        
                    }
        match errorModel with
        | { EmailError = ""; PasswordError = ""; ConfirmPasswordError = "" } ->
            let signedInUserEmail = ctx.User.Claims |> Seq.find (fun f -> f.Type = ClaimTypes.Name)
            let! dbUser = Procedures.Sel_UserByEmail dbConnection signedInUserEmail.Value
            match dbUser with
            | None -> return Error { errorModel with EmailError = "A strange error occured while processing." }
            | Some dbUser ->
                    let isValid = System.DateTime.UtcNow > dbUser.LockoutExpiry && (Password.verify dbUser.PasswordConfig model.Email dbUser.PasswordHash)
                            
                    let! r = Procedures.Upd_User_Login dbConnection dbUser.Id isValid

                    if isValid then
                        let passwordConfig = Password.makeDefaultPasswordConfig()
                        let hash = Password.generateHash passwordConfig model.Password
                        let! r = Procedures.UpdateUserPassword dbConnection dbUser.Id hash passwordConfig

                        return Ok ()
                    else if dbUser.LockoutExpiry > System.DateTime.UtcNow  then
                        return Error { errorModel with EmailError = "Log Me Out" }
                    else
                        return Error { errorModel with EmailError = "Invalid Password" }
                            
        | _ ->
                return Error errorModel
    }

let resetPasswordPostHandle (next : HttpFunc) (ctx : HttpContext) =
    task {
        let! modelResult = ctx.TryBindFormAsync<SignUp>()
        match modelResult with
        | Ok model -> 
                        let dbConnection = ctx.GetService<DbConnectionGetter>()
                        let! passwordResetResult = attemptPasswordReset dbConnection model ctx
                        match passwordResetResult with
                        | Error e ->
                                if e.EmailError = "Log Me Out" then
                                    do! ctx.SignOutAsync()
                                    return! (redirectTo false "/") earlyReturn ctx
                                else
                                    let verificationTag = createAntiForgeryTokenNode ctx
                                    return! (htmlView (HTMLViews.resetPasswordView verificationTag e model)) next ctx
                        | Ok _ -> return! (redirectTo false "/") next ctx
        | Error e ->
            return! setStatusCode 400 next ctx
    }

let envelopeIndexHandle (next : HttpFunc) (ctx : HttpContext) =
    task {
        let uid = Utils.getUserIDFromClaims ctx.User
        let dbConnection = ctx.GetService<DbConnectionGetter>()
        let! envelopes = Procedures.Sel_Envelope_Summary dbConnection uid
        return! (htmlView (HTMLViews.envelopeIndex envelopes)) next ctx
    }

let envelopeAddGetHandle (next : HttpFunc) (ctx : HttpContext) =
    
    let path = ctx.Request.Path.Value
    let antiForgeryField = createAntiForgeryTokenNode ctx
    if Utils.requestIsHTMX ctx then
        (htmlView (HTMLViews.addEnvelopePartial path antiForgeryField "" { EnvelopeName = System.String.Empty })) next ctx
    else
        (htmlView (HTMLViews.addEnvelopeFull path antiForgeryField "" { EnvelopeName = System.String.Empty })) next ctx

let envelopeAddPostHandle (next: HttpFunc) (ctx : HttpContext) =
    task {
        let! modelResult = ctx.TryBindFormAsync<EnvelopeName>()
        match modelResult with
        | Ok model -> 
                        let dbConnection = ctx.GetService<DbConnectionGetter>()
                        let uid = Utils.getUserIDFromClaims ctx.User
                        let! r = Procedures.Ins_Envelope dbConnection uid model.EnvelopeName
                        return! (redirectTo false "/Envelope") next ctx
        | Error e ->
            return! setStatusCode 400 next ctx
    }

let private envelopeUpdateGetHandle' (next : HttpFunc) (ctx : HttpContext) =

    let nameTextParam = ctx.TryGetQueryStringValue "name"
    let envelopeName =
        match nameTextParam with
        | None -> System.String.Empty
        | Some n -> n

    let path = ctx.Request.Path.Value
    let antiForgeryField = createAntiForgeryTokenNode ctx
    if Utils.requestIsHTMX ctx then
        (htmlView (HTMLViews.updateEnvelopePartial path antiForgeryField "" { EnvelopeName = envelopeName })) next ctx
    else
        (htmlView (HTMLViews.updateEnvelopeFull path antiForgeryField "" { EnvelopeName = envelopeName })) next ctx
    
let envelopeUpdateGetHandleChain (eidParam: int) = requireLogin >=> envelopeUpdateGetHandle'

let private envelopeUpdatePostHandle' (eidParam: int) (next: HttpFunc) (ctx : HttpContext) =
    task {
        let! modelResult = ctx.TryBindFormAsync<EnvelopeName>()
        match modelResult with
        | Ok model -> 
                        let dbConnection = ctx.GetService<DbConnectionGetter>()
                        let uid = Utils.getUserIDFromClaims ctx.User
                        let eid = eidParam |> int16
                        let! r = Procedures.Upd_Envelope dbConnection uid eid model.EnvelopeName
                        return! (redirectTo false "/Envelope") next ctx
        | Error e ->
            return! setStatusCode 400 next ctx
    }

let envelopeUpdatePostHandleChain (eidParam: int) =
    requireLogin >=> validateAntiForgeryToken >=> envelopeUpdatePostHandle' eidParam

let developeEnvelopeGetHandle' (eid: int) (next : HttpFunc) (ctx : HttpContext) =
    let nameTextParam = ctx.TryGetQueryStringValue "name"
    let envelopeName =
        match nameTextParam with
        | None -> System.String.Empty
        | Some n -> n

    let path = ctx.Request.Path.Value
    let antiForgeryField = createAntiForgeryTokenNode ctx
    if Utils.requestIsHTMX ctx then
        (htmlView (HTMLViews.deleteEnvelopePartial path antiForgeryField { EnvelopeName = envelopeName })) next ctx
    else
        (htmlView (HTMLViews.deleteEnvelopeFull path antiForgeryField { EnvelopeName = envelopeName })) next ctx

let deleteEnvelopeGetHandle (next : HttpFunc) (ctx : HttpContext) =
    let nameTextParam = ctx.TryGetQueryStringValue "name"
    let envelopeName =
        match nameTextParam with
        | None -> System.String.Empty
        | Some n -> n

    let path = ctx.Request.Path.Value
    let antiForgeryField = createAntiForgeryTokenNode ctx
    if Utils.requestIsHTMX ctx then
        (htmlView (HTMLViews.deleteEnvelopePartial path antiForgeryField { EnvelopeName = envelopeName })) next ctx
    else
        (htmlView (HTMLViews.deleteEnvelopeFull path antiForgeryField { EnvelopeName = envelopeName })) next ctx

let deleteEnvelopePostHandle (eid: int) (next : HttpFunc) (ctx : HttpContext) =
    task {
        let! modelResult = ctx.TryBindFormAsync<YesNo>()
        match modelResult with
        | Ok model -> 
                        match model.YesNo with
                        | Utils.IgnoreCaseEqual "Yes" ->
                            let dbConnection = ctx.GetService<DbConnectionGetter>()
                            let uid = Utils.getUserIDFromClaims ctx.User
                            let! r = Procedures.Del_Envelope dbConnection uid eid
                            return! (redirectTo false "/Envelope") next ctx
                        | Utils.IgnoreCaseEqual "No" -> return! (redirectTo false "/Envelope") next ctx
                        | _ -> return! deleteEnvelopeGetHandle next ctx
                            
        | Error e ->
            return! setStatusCode 400 next ctx
    }

let transactionGetHandle (eid32: int) (next : HttpFunc) (ctx : HttpContext) =
    task {
        let page = 
            let pageQuery = ctx.TryGetQueryStringValue("page")
            match pageQuery with
            | None -> 1
            | Some p ->
                    
                    let isValid, n = System.Int32.TryParse p
                    match isValid with
                    | false -> 1
                    | true -> System.Math.Max(1, n)
                    
        let eid = eid32 |> int16
        let uid = Utils.getUserIDFromClaims ctx.User
        let dbConnection = ctx.GetService<DbConnectionGetter>()
        let! transactionResult = Procedures.Sel_Transactions dbConnection uid eid Utils.DefaultPaginationSize page
        return! (htmlView (HTMLViews.transactionIndex eid ctx.Request.Path.Value page transactionResult)) next ctx
    }

let transactionCSV (eid32: int) (next : HttpFunc) (ctx : HttpContext) =
    task {
        let page = 1
        let eid = eid32 |> int16
        let uid = Utils.getUserIDFromClaims ctx.User
        let dbConnection = ctx.GetService<DbConnectionGetter>()
        let! transactionResult = Procedures.Sel_Transactions dbConnection uid eid System.Int32.MaxValue page
        
        let textWriter = new System.IO.StreamWriter(ctx.Response.Body, System.Text.Encoding.UTF8, leaveOpen = true)
        let writer = new CsvHelper.CsvWriter(textWriter, System.Globalization.CultureInfo.InvariantCulture, true)
        let! _ = writer.WriteRecordsAsync(transactionResult.Transactions)
        let! _ = writer.DisposeAsync()
        let! _ = textWriter.DisposeAsync()
        
        return! next ctx
    }

let private addTransactionGetHandle' (model: Models.Transaction) (eid32: int) (next : HttpFunc) (ctx : HttpContext) =
    let eid = eid32 |> int16
    let antiForgeryTag = createAntiForgeryTokenNode ctx
    let view =
        if Utils.requestIsHTMX ctx then
            HTMLViews.addTransactionViewPartial
        else
            HTMLViews.addTransactionView
    let v = htmlView (view ctx.Request.Path.Value eid antiForgeryTag model)
    v next ctx

let addTransactionGetHandle (eid32: int) (next : HttpFunc) (ctx : HttpContext) =
    addTransactionGetHandle' { TransactionNumber = 0; Amount = 0.0; Date = System.DateTime.MinValue; Note = ""; } eid32 next ctx

let addTransactionPostHandle (eid32: int) (next : HttpFunc) (ctx : HttpContext) =
    task {
        let eid = eid32 |> int16
        let! modelOption = ctx.TryBindFormAsync<Models.Transaction>()

        match modelOption with
        | Error e -> return! setStatusCode 400 next ctx
        | Ok model ->
            let uid = Utils.getUserIDFromClaims ctx.User
            let dbConnection = ctx.GetService<DbConnectionGetter>()
            let amountInCents = Utils.doubleMoneyToCents model.Amount
            let! transactionResult = Procedures.Ins_EnvelopeTransaction dbConnection uid eid amountInCents model.Date model.Note

            let hasAddAgain, hasAddAgainValue = ctx.Request.Form.TryGetValue "AddAgain"

            if hasAddAgain && hasAddAgainValue.Equals("on") then
                return! (addTransactionGetHandle' { model with Amount = 0.0; Note = System.String.Empty; } eid32) next ctx
            else if Utils.requestIsHTMX ctx then
                ctx.Response.Headers.Add("HX-Refresh", "true")
                return! (addTransactionGetHandle' model eid32) next ctx
            else
                return! (redirectTo false (sprintf "/Transaction/%i" eid32) next ctx)
    }

let updateTransactionGetHandle (eid32: int) (tid: int) (next : HttpFunc) (ctx : HttpContext) =
    task {
        let eid = eid32 |> int16
        let uid = Utils.getUserIDFromClaims ctx.User
        let dbConnection = ctx.GetService<DbConnectionGetter>()
        let antiForgeryTag = createAntiForgeryTokenNode ctx
        let! modelOption = Procedures.Sel_Transaction dbConnection uid eid tid
        let model = 
            match modelOption with
            | None -> { TransactionNumber = 0; Amount = 0.0; Date = System.DateTime.MinValue; Note = ""; }
            | Some m -> m

        let view =
            if Utils.requestIsHTMX ctx then
                HTMLViews.updateTransactionViewPartial
            else
                HTMLViews.updateTransactionViewFull
        let v = htmlView (view ctx.Request.Path.Value eid antiForgeryTag model)
        return! v next ctx
    }

let updateTransactionPostHandle (eid32: int) (tid: int) (next : HttpFunc) (ctx : HttpContext) =
    task {
        
        let! modelResult = ctx.TryBindFormAsync<Models.Transaction>()
        match modelResult with
        | Error e -> return! (setStatusCode 400) next ctx
        | Ok model ->

            let eid = eid32 |> int16
            let uid = Utils.getUserIDFromClaims ctx.User
            let dbConnection = ctx.GetService<DbConnectionGetter>()
            let amountInCents = Utils.doubleMoneyToCents model.Amount
            let! result = Procedures.Upd_EnvelopeTransaction dbConnection uid eid tid amountInCents model.Date model.Note
            
            let v = 
                if Utils.requestIsHTMX ctx then
                    ctx.Response.Headers.Add("HX-Refresh", "true")
                    addTransactionGetHandle eid32 // Since it will refresh, simply return a model without querying the database
                else
                    redirectTo false (sprintf "/Transaction/%i" eid)

            return! v next ctx
    }

let deleteTransactionGetViewHandle (eid32: int) (tid: int) (next : HttpFunc) (ctx : HttpContext) =
    task {
        let eid = eid32 |> int16
        let uid = Utils.getUserIDFromClaims ctx.User
        let dbConnection = ctx.GetService<DbConnectionGetter>()

        let! modelOption = Procedures.Sel_Transaction dbConnection uid eid tid
        let model =
            match modelOption with
            | None -> { TransactionNumber = 0; Amount = 0.0; Date = System.DateTime.MinValue; Note = System.String.Empty }
            | Some m -> m

        let view = if Utils.requestIsHTMX ctx then HTMLViews.deleteTransactionViewPartial else HTMLViews.deleteTransactionViewFull
        let antiForgeryNode = createAntiForgeryTokenNode ctx
        let v = htmlView (view ctx.Request.Path.Value antiForgeryNode model)

        return! v next ctx
    }
    
let deleteTransactionPostHandle (eid32: int) (tid: int) (next : HttpFunc) (ctx : HttpContext) =
    task {
        let! modelResult = ctx.TryBindFormAsync<YesNo>()
        match modelResult with
        | Ok model ->
            let redirectPath = sprintf "/Transaction/%i" eid32
            match model.YesNo with
            | Utils.IgnoreCaseEqual "Yes" ->
                let dbConnection = ctx.GetService<DbConnectionGetter>()
                let uid = Utils.getUserIDFromClaims ctx.User
                let eid = eid32 |> int16
                let! r = Procedures.Del_EnvelopeTransaction dbConnection uid eid tid
                let v = 
                    if Utils.requestIsHTMX ctx then
                        ctx.Response.Headers.Add("HX-Refresh", "true")
                        addTransactionGetHandle eid32 // Since it will refresh, simply return a model without querying the database
                    else
                        redirectTo false redirectPath
                return! v next ctx
            | Utils.IgnoreCaseEqual "No" -> return! (redirectTo false redirectPath) next ctx
            | _ -> return! (deleteTransactionGetViewHandle eid32 tid) next ctx
                            
        | Error e ->
            return! setStatusCode 400 next ctx
    }

let transferGetHandle (eid32: int) (next : HttpFunc) (ctx : HttpContext) =
    task {
        let uid = Utils.getUserIDFromClaims ctx.User
        let dbConnection = ctx.GetService<DbConnectionGetter>()
        let! envelopes = Procedures.Sel_Envelope_Summary dbConnection uid
        let eid = eid32 |> int16
        let envelopes = envelopes |> List.filter (fun f -> f.Number <> eid)
        let model = { DestinationNumber = 0s; Amount = 0.0; }
        let view = if Utils.requestIsHTMX ctx then HTMLViews.transferViewPartial else HTMLViews.transferViewFull
        let antiForgeryNode = createAntiForgeryTokenNode ctx
        let v = view ctx.Request.Path.Value antiForgeryNode envelopes model
        return! (htmlView v) next ctx
    }

let transferPostHandle (eid32: int) (next : HttpFunc) (ctx : HttpContext) =
    task {
        let! modelOption = ctx.TryBindFormAsync<Models.Transfer>()

        match modelOption with
        | Error s -> return! setStatusCode 400 next ctx
        | Ok model ->
            
            let uid = Utils.getUserIDFromClaims ctx.User
            let dbConnection = ctx.GetService<DbConnectionGetter>()
            let eid = eid32 |> int16
            let amountInCents = Utils.doubleMoneyToCents model.Amount
            let! dbResult = Procedures.Transfer dbConnection uid eid model.DestinationNumber amountInCents

            let view = 
                if Utils.requestIsHTMX ctx then
                    ctx.Response.Headers.Add("HX-Refresh", "true")
                    addTransactionGetHandle eid32 // Since it will refresh, simply return a model without querying the database
                else
                    redirectTo false (sprintf "/Transaction/%i" eid32)
            
            return! view next ctx
    }