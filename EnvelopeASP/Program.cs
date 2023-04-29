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
        connectionString = $"server={serverName};uid={uid};pwd={pwd};database={databaseName};";
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
