using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Text.Json;

namespace Duende.Bff.Tests.TestFramework
{
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
                endpoints.MapGet("/{**catch-all}", async context =>
                {
                    var sub = context.User.FindFirst(("sub"));
                    if (sub == null) throw new Exception("sub is missing");

                    var response = new
                    {
                        path = context.Request.Path.Value,
                        sub = sub,
                        claims = context.User.Claims.Select(x=>new { x.Type, x.Value }).ToArray()
                    };

                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                })
                .RequireAuthorization();
            });
        }
    }
}
