using Blazor.Client;
using Duende.Bff.Blazor.Client;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<IRenderModeContext, ClientRenderModeContext>();

builder.Services.AddBff();
builder.Services.AddRemoteApiHttpClient("callApi");

await builder.Build().RunAsync();
