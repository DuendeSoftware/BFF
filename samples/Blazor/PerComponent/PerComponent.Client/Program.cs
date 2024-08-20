using PerComponent.Client;
using Duende.Bff.Blazor.Client;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<IRenderModeContext, ClientRenderModeContext>();

builder.Services
    .AddBffBlazorClient()
    .AddCascadingAuthenticationState()
    .AddRemoteApiHttpClient("callApi");

await builder.Build().RunAsync();
