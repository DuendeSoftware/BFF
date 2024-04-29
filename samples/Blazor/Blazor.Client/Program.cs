using Blazor.Client;
using Blazor.Client.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<IRenderModeContext, ClientRenderModeContext>();

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, BffAuthenticationStateProvider>();


// HTTP client configuration
builder.Services.AddTransient<AntiforgeryHandler>();
builder.Services.AddHttpClient<BffAuthenticationStateProvider>(client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<AntiforgeryHandler>();

// TODO - Is it possible to use the typed style and not need an http client factory in the components?
builder.Services.AddHttpClient("callApi", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "remote-apis/user-token/"))
    .AddHttpMessageHandler<AntiforgeryHandler>();

await builder.Build().RunAsync();
