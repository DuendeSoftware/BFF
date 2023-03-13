using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
        .MinimumLevel.Override("IdentityModel", LogEventLevel.Debug)
        .MinimumLevel.Override("Duende.Bff", LogEventLevel.Debug)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
            theme: AnsiConsoleTheme.Code));
    
    builder.Services.AddControllers();
    builder.Services.AddRazorPages();
    builder.Services.AddBff();
    
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = "cookie";
            options.DefaultChallengeScheme = "oidc";
            options.DefaultSignOutScheme = "oidc";
        })
        .AddCookie("cookie", options =>
        {
            options.Cookie.Name = "__Host-blazor";

            // If your authority is on a different domain from your SPA (blazor in this case), you might want to use Lax instead of Strict
            // See https://github.com/DuendeSoftware/Support/issues/565
            options.Cookie.SameSite = SameSiteMode.Strict;
        })
        .AddOpenIdConnect("oidc", options =>
        {
            options.Authority = "https://localhost:5001";
    
            // confidential client using code flow + PKCE
            options.ClientId = "spa";
            options.ClientSecret = "secret";
            options.ResponseType = "code";
            options.ResponseMode = "query";
    
            options.MapInboundClaims = false;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.SaveTokens = true;
    
            // request scopes + refresh tokens
            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("api");
            options.Scope.Add("offline_access");
        });
    
    var app = builder.Build();

    app.UseSerilogRequestLogging();
    
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
    }
    else
    {
        app.UseExceptionHandler("/Error");
    }
    
    app.UseBlazorFrameworkFiles();
    app.UseStaticFiles();
    
    app.UseRouting();
    app.UseAuthentication();
    app.UseBff();
    app.UseAuthorization();
    
    app.MapBffManagementEndpoints();
    app.MapRazorPages();

    app.MapControllers()
        .RequireAuthorization()
        .AsBffApiEndpoint();
    
    app.MapFallbackToFile("index.html");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
