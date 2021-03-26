using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Duende.Bff.Tests.TestFramework
{
    public record ApiResponse(string method, string path, string sub, IEnumerable<ClaimRecord> claims, string body = null);

    public class ApiHost : GenericHost
    {
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
                    var sub = context.User.FindFirst(("sub"))?.Value;
                    if (sub == null) throw new Exception("sub is missing");

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
                        sub,
                        context.User.Claims.Select(x => new ClaimRecord(x.Type, x.Value)).ToArray(),
                        body
                    );

                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                })
                .RequireAuthorization();
            });
        }
    }
}
