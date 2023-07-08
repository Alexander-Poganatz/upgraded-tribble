namespace EnvelopeWeb.Controllers


open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Models
open System.Threading.Tasks
open System.Security.Claims
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication
open Microsoft.AspNetCore.Authorization


type AccountController (logger : ILogger<AccountController>, dbConnectionGetter : DbConnection.IDbConnectionGetter) =
    inherit Controller()

    member this.Index () =
        this.View()

    member this.SignUp (model:Models.SignUp) : Task<IActionResult> =
        if this.Request.Method = "POST" && this.ModelState.IsValid then
            let viewBag = this.ViewData
            if model.Password.Length > 1024 then
                viewBag["Error"] <- "The password is a bit big, rejected"
                task { return this.View(model) }
            else if model.Password <> model.ConfirmPassword then
                viewBag["Error"] <- "Passwords do not match"
                task { return this.View(model) }
            else
                
                let passwordConfig = Password.makeDefaultPasswordConfig()
                let hash = Password.generateHash passwordConfig model.Password
                let email = model.Email.Trim()
                async {

                    let! t = Procedures.InsertUser dbConnectionGetter email hash passwordConfig

                    return this.Redirect("Login") :> IActionResult
                } |> Async.StartAsTask

        else
            task { return this.View(model) }


    member this.Login (model: Login) (returnUrl : string) : Task<IActionResult> =
        let returnUrl = if System.String.IsNullOrEmpty(returnUrl) then "/" else returnUrl
        if this.User.Identity.IsAuthenticated then
            task { return this.Redirect(returnUrl) }
        else if this.Request.Method = "POST" && this.ModelState.IsValid then
            let email = model.Email.Trim()
            async {
                let! dbUserN = Procedures.Sel_UserByEmail dbConnectionGetter email

                let returnResult : Async<IActionResult> =
                    match dbUserN with
                    | None -> async { return this.View(model) }
                    | Some dbUser ->
                            let isValid = System.DateTime.UtcNow > dbUser.LockoutExpiry && (Password.verify dbUser.PasswordConfig model.Password dbUser.PasswordHash)
                            async {
                                let! r = Procedures.Upd_User_Login dbConnectionGetter dbUser.Id isValid

                                if isValid then 
                                    let idClaim = new Claim(ClaimTypes.NameIdentifier, dbUser.Id.ToString())
                                    let nameClaim = new Claim(ClaimTypes.Name, email)
                                    let claims = [| idClaim; nameClaim; |]
                                    let claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
                                    let principle = new ClaimsPrincipal(claimsIdentity)

                                    let! r = this.HttpContext.SignInAsync(principle) |> Async.AwaitTask

                                    return this.Redirect(returnUrl)
                                else
                                    return this.View(model)
                            }
                            

                return! returnResult
                
            } |> Async.StartAsTask
        else
            task { return this.View(model) }

    member this.Logout() =
        async {
            if this.User.Identity.IsAuthenticated then
                do! this.HttpContext.SignOutAsync() |> Async.AwaitTask

            return this.Redirect("/")
        } |> Async.StartAsTask

    [<Authorize>]
    member this.PasswordReset (model:SignUp) : Task<IActionResult> =
        if this.Request.Method = "POST" && this.ModelState.IsValid then
            let viewBag = this.ViewData
           
            if model.Password.Length > 1024 then
                viewBag["Error"] <- "The new password is a bit big, rejected"
                task { return this.View(model) }
            else if model.Password <> model.ConfirmPassword then
                viewBag["Error"] <- "The new passwords do not match"
                task { return this.View(model) }
            else
                // check old password
                async {
                    let emailClaim = this.User.Claims |> Seq.find (fun f -> f.Type = ClaimTypes.Name)
                    let! optUser = Procedures.Sel_UserByEmail dbConnectionGetter emailClaim.Value

                    match optUser with
                    | None -> 
                        viewBag["Error"] <- "Strange user not found error occured"
                        return this.View(model) :> IActionResult
                    | Some dbUser ->
                        let isValid = System.DateTime.UtcNow > dbUser.LockoutExpiry && (Password.verify dbUser.PasswordConfig model.Password dbUser.PasswordHash)

                        return! async {
                            let! r = Procedures.Upd_User_Login dbConnectionGetter dbUser.Id isValid

                            if isValid then 
                                let passwordConfig = Password.makeDefaultPasswordConfig()
                                let hash = Password.generateHash passwordConfig model.Password
                                let uid = Utils.getUserIDFromClaims this.User
                                let! r = Procedures.UpdateUserPassword dbConnectionGetter uid hash passwordConfig

                                return this.Redirect("/") :> IActionResult
                            else
                                viewBag["Error"] <- "Current password is invalid"
                                return this.View(model) :> IActionResult
                        }

                } |> Async.StartAsTask
        else
            task { return this.View(model) }