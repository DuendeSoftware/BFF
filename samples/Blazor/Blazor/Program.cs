using Blazor;
using Blazor.Client;
using Blazor.Components;
using Duende.AccessTokenManagement.OpenIdConnect;
using Duende.Bff;
using Duende.Bff.Blazor;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.Authentication;

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

        options.SignOutScheme = "cookie";
    });

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

app.UseRouting();
app.UseAuthentication();
app.UseBff();
app.UseAuthorization();
app.UseAntiforgery();

app.MapBffManagementEndpoints();

// We don't use the BFF endpoint for logout because it might not be convenient
// to use, depending on the render mode. In wasm, we retrieve the bff management
// claims so we would know where to logout. But in SSR or blazor server, we
// won't have bff management claims easily. We can always just render a link to
// an endpoint with an antiforgery token though. And this works even in
// InteractiveWasm mode, because there is an initial SSR. If using wasm globally
// without InteractiveWasm (so no initial SSR), then use the BFF management
// claim instead.
//
// TODO - maybe we can't assume that the logout button will be rendered via SSR.
// What if some complex user interaction causes it to be displayed?
app.MapPost("/logout", async (HttpContext context) =>
{
    // We have to revoke the refresh token before we call SignOutAsync instead of
    // in the SigningOut cookie handler event. That event is called after the
    // ticket store has already deleted the ticket. So, in the cookie event we won't
    // be able to authenticate again in order to get at the authentication properties
    // and get at the refresh token.
    await context.RevokeRefreshTokenAsync();
    await context.SignOutAsync();
});

app.MapRemoteBffApiEndpoint("/remote-apis/user-token", "https://localhost:5010")
    .RequireAccessToken(TokenType.User);

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Blazor.Client._Imports).Assembly);

app.Run();
