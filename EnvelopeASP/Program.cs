using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
{
    var iniPath = builder.Configuration["dbINIPath"];
    if (string.IsNullOrWhiteSpace(iniPath) is false)
    {
        builder.Configuration.AddIniFile(iniPath, true);
    }
}

var connectionString = builder.Configuration["dbConnection"] ?? string.Empty;
{
    var mariaEnvelopeSection = builder.Configuration.GetSection("MariaEnvelope");
    if(mariaEnvelopeSection != null)
    {
        var serverName = mariaEnvelopeSection["Servername"];
        var databaseName = mariaEnvelopeSection["Database"];
        var uid = mariaEnvelopeSection["UID"];
        var pwd = mariaEnvelopeSection["PWD"];
        // Somehow get to here even though no section is present
        if(serverName != null)
        {
            connectionString = $"server={serverName};uid={uid};pwd={pwd};database={databaseName};";
        }

    }
}

EnvelopeASP.Models.Procedures.SetConnectionString(connectionString);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";

    });

//bool useForwardHeaders = Convert.ToBoolean(builder.Configuration["useForwardHeaders"]);
bool enforceHTTPSRedirection = Convert.ToBoolean(builder.Configuration["enforceHTTPSRedirection"]);

var app = builder.Build();

// I started copying from Microsoft tutorial, but after looking at what I was doing, I currently have no use for originating ip addresses and protocols.
/*
if (useForwardHeaders)
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
    });
}
*/

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (enforceHTTPSRedirection)
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
