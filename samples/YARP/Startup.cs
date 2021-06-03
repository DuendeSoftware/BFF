// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Threading.Tasks;
using Duende.Bff;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Yarp.ReverseProxy.Abstractions;
using Yarp.ReverseProxy.Abstractions.Config;
using Yarp.ReverseProxy.Middleware;
using Yarp.ReverseProxy.RuntimeModel;
using Yarp.Sample;
using YARP.Sample;

namespace Host5
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var builder = services.AddReverseProxy()
                .AddTransforms<AccessTokenTransformProvider>();

            builder.LoadFromMemory(
                new[]
                {
                    new RouteConfig()
                    {
                        RouteId = "api",
                        ClusterId = "cluster1",
                        
                        Match = new RouteMatch
                        {
                            Path = "/api/{**catch-all}"
                        }
                    }
                    .WithAccessToken(TokenType.User)
                },
                new[]
                {
                    new ClusterConfig
                    {
                        ClusterId = "cluster1",

                        Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
                        {
                            { "destination1", new DestinationConfig() { Address = "https://localhost:5010" } },
                        }
                    }
                });


            // Add BFF services to DI - also add server-side session management
            services.AddBff()
                .AddServerSideSessions();

            // local APIs
            services.AddControllers();

            // cookie options
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "cookie";
                    options.DefaultChallengeScheme = "oidc";
                    options.DefaultSignOutScheme = "oidc";
                })
                .AddCookie("cookie", options =>
                {
                    // set session lifetime
                    options.ExpireTimeSpan = TimeSpan.FromHours(8);

                    // sliding or absolute
                    options.SlidingExpiration = false;

                    // host prefixed cookie name
                    options.Cookie.Name = "__Host-spa5";

                    // strict SameSite handling
                    options.Cookie.SameSite = SameSiteMode.Strict;
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://localhost:5001";

                    // confidential client using code flow + PKCE
                    options.ClientId = "spa";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.ResponseMode = "query";

                    options.MapInboundClaims = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    // request scopes + refresh tokens
                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("api");
                    options.Scope.Add("offline_access");
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseSerilogRequestLogging();
            app.UseDeveloperExceptionPage();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseRouting();

            // adds antiforgery protection for local APIs
            app.UseBff();

            // adds authorization for local and remote API endpoints
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                // local APIs
                endpoints.MapControllers()
                    .RequireAuthorization()
                    .AsLocalBffApiEndpoint();

                // login, logout, user, backchannel logout...
                endpoints.MapBffManagementEndpoints();

                endpoints.MapReverseProxy(pipeline =>
                {
                    pipeline.AddAntiforgeryProtection();
                });
            });
        }

        private Task CustomProxyStep(HttpContext context, Func<Task> next)
        {
            var feature = context.Features.Get<IReverseProxyFeature>();

            return next();
        }
    }
}