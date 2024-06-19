using Duende.Bff.Blazor.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// authentication state and authorization
builder.Services.AddBff();

await builder.Build().RunAsync();
