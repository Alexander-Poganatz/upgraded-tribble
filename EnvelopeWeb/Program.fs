namespace EnvelopeWeb

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Authentication.Cookies
open Giraffe

module Program =

    let private errorHandle (ex: Exception) (logger: ILogger) =
        logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
        clearResponse >=> setStatusCode 500 >=> text ex.Message

    let exitCode = 0

    let time() = System.DateTime.Now.ToString()

    let webApp =
        choose [
            GET >=> choose [
                route "/" >=> HTMLViewHandles.homeViewHandle
                route "/Account/Login" >=> HTMLViewHandles.requireSignOut >=> HTMLViewHandles.loginGetHandle
                route "/Account/Logout" >=> signOut CookieAuthenticationDefaults.AuthenticationScheme >=> redirectTo false "/"
                route "/Account/SignUp" >=> HTMLViewHandles.requireSignOut >=> HTMLViewHandles.signUpGetHandle
                route "/Account/PasswordReset" >=> HTMLViewHandles.requireLogin >=> HTMLViewHandles.resetPasswordGetHandle
                route "/Envelope" >=> HTMLViewHandles.requireLogin >=> HTMLViewHandles.envelopeIndexHandle
                route "/Envelope/Add" >=> HTMLViewHandles.requireLogin >=> HTMLViewHandles.envelopeAddGetHandle
                routef "/Envelope/Update/%i" HTMLViewHandles.envelopeUpdateGetHandleChain
                routef "/Envelope/Delete/%i" (fun eid -> HTMLViewHandles.requireLogin >=> HTMLViewHandles.developeEnvelopeGetHandle' eid)
                routef "/Transaction/%i" (fun eid -> HTMLViewHandles.requireLogin >=> HTMLViewHandles.transactionGetHandle eid)
                routef "/Transaction/Add/%i" (fun eid -> HTMLViewHandles.requireLogin >=> HTMLViewHandles.addTransactionGetHandle eid)
                routef "/Transaction/Update/%i/%i" (fun (eid, tid) -> HTMLViewHandles.requireLogin >=> HTMLViewHandles.updateTransactionGetHandle eid tid)
                routef "/Transaction/Delete/%i/%i" (fun (eid, tid) -> HTMLViewHandles.requireLogin >=> HTMLViewHandles.deleteTransactionGetViewHandle eid tid)
                routef "/Transaction/Transfer/%i" (fun eid -> HTMLViewHandles.requireLogin >=> HTMLViewHandles.transferGetHandle eid)
            ];
            POST >=> choose [
                route "/Account/Login" >=> HTMLViewHandles.requireSignOut >=> HTMLViewHandles.validateAntiForgeryToken >=> HTMLViewHandles.loginPostHandle
                route "/Account/SignUp" >=> HTMLViewHandles.requireSignOut >=> HTMLViewHandles.validateAntiForgeryToken >=> HTMLViewHandles.signUpPostHandle
                route "/Account/PasswordReset" >=> HTMLViewHandles.requireLogin >=> HTMLViewHandles.validateAntiForgeryToken >=> HTMLViewHandles.resetPasswordPostHandle
                route "/Envelope/Add" >=> HTMLViewHandles.requireLogin >=> HTMLViewHandles.validateAntiForgeryToken >=> HTMLViewHandles.envelopeAddPostHandle
                routef "/Envelope/Update/%i" HTMLViewHandles.envelopeUpdatePostHandleChain
                routef "/Envelope/Delete/%i" (fun eid -> HTMLViewHandles.requireLogin >=> HTMLViewHandles.validateAntiForgeryToken >=> HTMLViewHandles.deleteEnvelopePostHandle eid)
                routef "/Transaction/Add/%i" (fun eid -> HTMLViewHandles.requireLogin >=> HTMLViewHandles.validateAntiForgeryToken >=> HTMLViewHandles.addTransactionPostHandle eid)
                routef "/Transaction/Update/%i/%i" (fun (eid, tid) -> HTMLViewHandles.requireLogin >=> HTMLViewHandles.validateAntiForgeryToken >=> HTMLViewHandles.updateTransactionPostHandle eid tid)
                routef "/Transaction/Delete/%i/%i" (fun (eid, tid) -> HTMLViewHandles.requireLogin >=> HTMLViewHandles.validateAntiForgeryToken >=> HTMLViewHandles.deleteTransactionPostHandle eid tid)
                routef "/Transaction/Transfer/%i" (fun eid -> HTMLViewHandles.requireLogin >=> HTMLViewHandles.validateAntiForgeryToken >=> HTMLViewHandles.transferPostHandle eid)
            ]
            
        ]

    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)

        let connectionString = builder.Configuration["dbConnection"]

        builder.Services.AddGiraffe |> ignore

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(fun options ->
            
                    options.LoginPath <- "/Account/Login"
                    options.LogoutPath <- "/Account/Logout"
                    options.AccessDeniedPath <- "/Account/Login"

                )
            |> ignore

        builder.Services.AddAuthorization() |> ignore
        builder.Services.AddAntiforgery() |> ignore

        let useForwardHeaders = Convert.ToBoolean(builder.Configuration["useForwardHeaders"])
        let enforceHTTPSRedirection = Convert.ToBoolean(builder.Configuration["enforceHTTPSRedirection"])

        let dbConnectionGetter = new DbConnection.DbConnectionGetter(connectionString)

        builder.Services.AddSingleton(dbConnectionGetter) |> ignore

        let app = builder.Build()

        if not (builder.Environment.IsDevelopment()) then
            if useForwardHeaders then
                app.UseForwardedHeaders(ForwardedHeadersOptions(ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto)) |> ignore

            app.UseHsts() |> ignore // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        
        if (enforceHTTPSRedirection) then
            app.UseHttpsRedirection() |> ignore

        app.UseGiraffeErrorHandler errorHandle |> ignore
        app.UseStaticFiles() |> ignore
        app.UseAuthentication() |> ignore
        app.UseAuthorization() |> ignore

        app.UseGiraffe webApp

        app.Run()

        exitCode
