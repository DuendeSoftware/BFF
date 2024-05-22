using Blazor.Client;
using Duende.Bff.Blazor.Wasm;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddScoped<IRenderModeContext, ClientRenderModeContext>();

builder.Services.AddBff();
builder.Services.AddRemoteApiHttpClient("callApi", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress + "remote-apis/"));

await builder.Build().RunAsync();
