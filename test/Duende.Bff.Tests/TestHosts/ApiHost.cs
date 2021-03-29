// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff.Tests.TestFramework;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Duende.Bff.Tests.TestHosts
{
    public class ApiHost : GenericHost
    {
        public int? ApiStatusCodeToReturn { get; set; }

        private readonly IdentityServerHost _identityServerHost;

        public ApiHost(IdentityServerHost identityServerHost, string scope, string baseAddress = "https://api") 
            : base(baseAddress)
        {
            _identityServerHost = identityServerHost;

            _identityServerHost.ApiScopes.Add(new ApiScope(scope));

            OnConfigureServices += ConfigureServices;
            OnConfigure += Configure;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddAuthorization();

            services.AddAuthentication("token")
                .AddJwtBearer("token", options =>
                {
                    options.Authority = _identityServerHost.Url();
                    options.Audience = _identityServerHost.Url("/resources");
                    options.MapInboundClaims = false;
                    options.BackchannelHttpHandler = _identityServerHost.Server.CreateHandler();
                });
        }

        private void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/{**catch-all}", async context =>
                {
                    var body = default(string);
                    if (context.Request.HasJsonContentType())
                    {
                        using (var sr = new StreamReader(context.Request.Body))
                        {
                            body = await sr.ReadToEndAsync();
                        }
                    }

                    var response = new ApiResponse(
                        context.Request.Method,
                        context.Request.Path.Value,
                        context.User.FindFirst(("sub"))?.Value,
                        context.User.FindFirst(("client_id"))?.Value,
                        context.User.Claims.Select(x => new ClaimRecord(x.Type, x.Value)).ToArray(),
                        body
                    );

                    context.Response.StatusCode = ApiStatusCodeToReturn ?? 200;
                    ApiStatusCodeToReturn = null;

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                });
            });
        }
    }
}
