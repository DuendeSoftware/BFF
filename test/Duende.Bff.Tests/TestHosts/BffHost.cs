// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff.Tests.TestFramework;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CsQuery.ExtensionMethods;
using Microsoft.Extensions.Primitives;

namespace Duende.Bff.Tests.TestHosts
{
    public class BffHost : GenericHost
    {
        public int? LocalApiStatusCodeToReturn { get; set; }

        private readonly IdentityServerHost _identityServerHost;
        private readonly ApiHost _apiHost;
        private readonly string _clientId;

        public BffOptions BffOptions { get; set; } = new();

        public BffHost(IdentityServerHost identityServerHost, ApiHost apiHost, string clientId, string baseAddress = "https://app")
            : base(baseAddress)
        {
            _identityServerHost = identityServerHost;
            _apiHost = apiHost;
            _clientId = clientId;

            OnConfigureServices += ConfigureServices;
            OnConfigure += Configure;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddAuthorization();

            var bff = services.AddBff();
            services.AddSingleton(BffOptions);

            bff.ConfigureTokenClient()
                .ConfigurePrimaryHttpMessageHandler(() => _identityServerHost.Server.CreateHandler());

            services.AddSingleton<IHttpMessageInvokerFactory>(
                new CallbackHttpMessageInvokerFactory(
                    path => new HttpMessageInvoker(_apiHost.Server.CreateHandler())));

            services.AddAuthentication("cookie")
                .AddCookie("cookie");

            bff.AddServerSideSessions();

            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = "oidc";
                options.DefaultSignOutScheme = "oidc";
            })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = _identityServerHost.Url();

                    options.ClientId = _clientId;
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.ResponseMode = "query";

                    options.MapInboundClaims = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.Scope.Clear();
                    var client = _identityServerHost.Clients.Single(x => x.ClientId == _clientId);
                    foreach (var scope in client.AllowedScopes)
                    {
                        options.Scope.Add(scope);
                    }
                    if (client.AllowOfflineAccess)
                    {
                        options.Scope.Add("offline_access");
                    }

                    options.BackchannelHttpHandler = _identityServerHost.Server.CreateHandler();
                });

            services.AddAuthorization(options => {
                options.AddPolicy("AlwaysFail", policy => {
                    policy.RequireAssertion(ctx => false);
                });
            });
        }

        private void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();

            app.UseRouting();

            app.UseBff();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBffManagementEndpoints();
                
                endpoints.Map("/local_anon", async context =>
                {
                    var body = default(string);
                    if (context.Request.HasJsonContentType())
                    {
                        using (var sr = new StreamReader(context.Request.Body))
                        {
                            body = await sr.ReadToEndAsync();
                        }
                    }

                    var headers = new Dictionary<string, List<string>>();
                    foreach (var header in context.Request.Headers)
                    {
                        var values = new List<string>(header.Value.Select(v => v));
                        headers.Add(header.Key, values);
                    }

                    var response = new ApiResponse(
                        context.Request.Method,
                        context.Request.Path.Value,
                        context.User.FindFirst(("sub"))?.Value,
                        context.User.FindFirst(("client_id"))?.Value,
                        context.User.Claims.Select(x => new ClaimRecord(x.Type, x.Value)).ToArray())
                        {
                            Body = body,
                            RequestHeaders = headers
                        };

                    context.Response.StatusCode = LocalApiStatusCodeToReturn ?? 200;
                    LocalApiStatusCodeToReturn = null;

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                })
                    .AsLocalBffApiEndpoint();

                endpoints.Map("/local_authz", async context =>
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
                        context.User.FindFirst(("client_id"))?.Value,
                        context.User.Claims.Select(x => new ClaimRecord(x.Type, x.Value)).ToArray())
                    {
                        Body = body
                    };
                    
                    context.Response.StatusCode = LocalApiStatusCodeToReturn ?? 200;
                    LocalApiStatusCodeToReturn = null;

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                })
                    .RequireAuthorization()
                    .AsLocalBffApiEndpoint();


                endpoints.Map("/always_fail_authz_non_bff_endpoint", context =>
                {
                    return Task.CompletedTask;
                })
                    .RequireAuthorization("AlwaysFail");

                endpoints.Map("/always_fail_authz", context =>
                {
                    return Task.CompletedTask;
                })
                    .AsLocalBffApiEndpoint()
                    .RequireAuthorization("AlwaysFail");


                endpoints.MapRemoteBffApiEndpoint("/api_user", _apiHost.Url())
                    .RequireAccessToken();

                endpoints.MapRemoteBffApiEndpoint("/api_user_no_csrf", _apiHost.Url())
                    .RequireAccessToken()
                    .DisableAntiforgeryProtection();

                endpoints.MapRemoteBffApiEndpoint("/api_client", _apiHost.Url())
                    .RequireAccessToken(TokenType.Client);

                endpoints.MapRemoteBffApiEndpoint("/api_user_or_client", _apiHost.Url())
                    .RequireAccessToken(TokenType.UserOrClient);

                endpoints.MapRemoteBffApiEndpoint("/api_user_or_anon", _apiHost.Url())
                    .WithOptionalUserAccessToken();

                endpoints.MapRemoteBffApiEndpoint("/api_anon_only", _apiHost.Url());

                endpoints.Map("/not_bff_endpoint", BffRemoteApiEndpoint.Map("/not_bff_endpoint", _apiHost.Url()));
            });

            app.Map("/invalid_endpoint", invalid => invalid.Use(next => BffRemoteApiEndpoint.Map("/invalid_endpoint", _apiHost.Url())));
        }

        public async Task<bool> GetIsUserLoggedInAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, Url("/bff/user"));
            req.Headers.Add("x-csrf", "1");
            var response = await BrowserClient.SendAsync(req);

            (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized).Should().BeTrue();

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<ClaimRecord[]> GetUserClaimsAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, Url("/bff/user"));
            req.Headers.Add("x-csrf", "1");

            var response = await BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(200);
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");

            var json = await response.Content.ReadAsStringAsync();
            var claims = JsonSerializer.Deserialize<ClaimRecord[]>(json);

            return claims;
        }


        public async Task<HttpResponseMessage> BffLoginAsync(string sub, string sid = null)
        {
            await _identityServerHost.CreateIdentityServerSessionCookieAsync(sub, sid);

            var response = await BrowserClient.GetAsync(Url("/bff/login"));
            response.StatusCode.Should().Be(302); // authorize
            response.Headers.Location.ToString().ToLowerInvariant().Should().StartWith(_identityServerHost.Url("/connect/authorize"));

            response = await _identityServerHost.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(302); // client callback
            response.Headers.Location.ToString().ToLowerInvariant().Should().StartWith(Url("/signin-oidc"));

            response = await BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(302); // root
            response.Headers.Location.ToString().ToLowerInvariant().Should().Be("/");

            (await GetIsUserLoggedInAsync()).Should().BeTrue();

            response = await BrowserClient.GetAsync(Url(response.Headers.Location.ToString()));
            return response;
        }

        public async Task<HttpResponseMessage> BffLogoutAsync(string sid = null)
        {
            var response = await BrowserClient.GetAsync(Url("/bff/logout") + "?sid=" + sid);
            response.StatusCode.Should().Be(302); // endsession
            response.Headers.Location.ToString().ToLowerInvariant().Should().StartWith(_identityServerHost.Url("/connect/endsession"));

            response = await _identityServerHost.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(302); // logout
            response.Headers.Location.ToString().ToLowerInvariant().Should().StartWith(_identityServerHost.Url("/account/logout"));

            response = await _identityServerHost.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(302); // post logout redirect uri
            response.Headers.Location.ToString().ToLowerInvariant().Should().StartWith(Url("/signout-callback-oidc"));

            response = await BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(302); // root
            response.Headers.Location.ToString().ToLowerInvariant().Should().Be("/");

            (await GetIsUserLoggedInAsync()).Should().BeFalse();

            response = await BrowserClient.GetAsync(Url(response.Headers.Location.ToString()));
            return response;
        }

        public class CallbackHttpMessageInvokerFactory : IHttpMessageInvokerFactory
        {
            public CallbackHttpMessageInvokerFactory(Func<string, HttpMessageInvoker> callback)
            {
                CreateInvoker = callback;
            }
            public Func<string, HttpMessageInvoker> CreateInvoker { get; set; }
            public HttpMessageInvoker CreateClient(string localPath)
            {
                return CreateInvoker.Invoke(localPath);
            }
        }
    }
}
