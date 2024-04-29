using Blazor;
using Blazor.Client;
using Blazor.Components;
using Duende.Bff;
using Duende.Bff.Blazor;
using Duende.Bff.Yarp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddBff()
    .AddServerSideSessions()
    .AddRemoteApis();

builder.Services.AddCascadingAuthenticationState();

// adds access token management
builder.Services.AddOpenIdConnectAccessTokenManagement()
    .AddBlazorServerAccessTokenManagement<ServerSideTokenStore>();

builder.Services.AddScoped<IRenderModeContext, ServerRenderModeContext>();
builder.Services.AddUserAccessTokenHttpClient("callApi", configureClient: client => client.BaseAddress = new Uri("https://localhost:5010/"));


builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "cookie";
        options.DefaultChallengeScheme = "oidc";
        options.DefaultSignOutScheme = "oidc";
    })
    .AddCookie("cookie", options =>
    {
        options.Cookie.Name = "__Host-blazor";
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.EventsType = typeof(CookieEvents);
    })
    .AddOpenIdConnect("oidc", options =>
    {
        options.Authority = "https://localhost:5001";

        options.ClientId = "blazor";
        options.ClientSecret = "secret";
        options.ResponseType = "code";
        options.ResponseMode = "query";

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("api");
        options.Scope.Add("offline_access");

        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = "name";
        options.TokenValidationParameters.RoleClaimType = "role";

        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;
    });

// register events to customize authentication handlers
// TODO - Register events in a new helper in Duende.Bff.Blazor
builder.Services.AddTransient<CookieEvents>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.UseRouting();
app.UseAntiforgery();
app.UseAuthentication();
app.UseBff();
app.UseAuthorization();

app.MapBffManagementEndpoints();

app.MapRemoteBffApiEndpoint("/remote-apis/user-token", "https://localhost:5010")
    .RequireAccessToken(TokenType.User);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Blazor.Client._Imports).Assembly);

app.Run();
