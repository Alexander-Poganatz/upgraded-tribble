namespace EnvelopeWeb

open System
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.Authentication.Cookies

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)

        let iniPath = builder.Configuration["dbINIPath"]

        let connectionString = 
            let connectionString' = builder.Configuration["dbConnection"]
            if connectionString' |> System.String.IsNullOrWhiteSpace then 
                let mariaEnvelopeSection = builder.Configuration.GetSection("MariaEnvelope");
            
                let serverName = mariaEnvelopeSection["Servername"];
                let databaseName = mariaEnvelopeSection["Database"];
                let uid = mariaEnvelopeSection["UID"];
                let pwd = mariaEnvelopeSection["PWD"];
                    
                $"server={serverName};uid={uid};pwd={pwd};database={databaseName};";
                    
            else
                connectionString'

        builder
            .Services
            .AddControllersWithViews()
            .AddRazorRuntimeCompilation()
            |> ignore

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(fun options ->
            
                    options.LoginPath <- "/Account/Login"
                    options.LogoutPath <- "/Account/Logout"

                )
            |> ignore

        let useForwardHeaders = Convert.ToBoolean(builder.Configuration["useForwardHeaders"])
        let enforceHTTPSRedirection = Convert.ToBoolean(builder.Configuration["enforceHTTPSRedirection"])

        builder.Services.AddRazorPages() |> ignore

        let dbConnectionGetter = new DbConnection.DbConnectionGetter(connectionString) :> DbConnection.IDbConnectionGetter

        builder.Services.AddSingleton(dbConnectionGetter) |> ignore

        let app = builder.Build()

        if not (builder.Environment.IsDevelopment()) then
            app.UseExceptionHandler("/Home/Error") |> ignore

            if useForwardHeaders then
                app.UseForwardedHeaders(ForwardedHeadersOptions(ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto)) |> ignore

            app.UseHsts() |> ignore // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        
        if (enforceHTTPSRedirection) then
            app.UseHttpsRedirection() |> ignore

        app.UseStaticFiles() |> ignore
        app.UseRouting() |> ignore
        app.UseAuthorization() |> ignore

        app.MapControllerRoute(name = "default", pattern = "{controller=Home}/{action=Index}/{id?}") |> ignore

        app.MapRazorPages() |> ignore

        app.Run()

        exitCode
