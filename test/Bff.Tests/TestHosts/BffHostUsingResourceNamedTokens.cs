﻿// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff.Tests.TestFramework;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Duende.Bff.Tests.TestHosts
{
    public class BffHostUsingResourceNamedTokens : GenericHost
    {
        public int? LocalApiStatusCodeToReturn { get; set; }

        private readonly IdentityServerHost _identityServerHost;
        private readonly ApiHost _apiHost;
        private readonly string _clientId;
        private readonly bool _useForwardedHeaders;

        public BffOptions BffOptions { get; private set; }

        public BffHostUsingResourceNamedTokens(IdentityServerHost identityServerHost, ApiHost apiHost, string clientId,
            string baseAddress = "https://app", bool useForwardedHeaders = false)
            : base(baseAddress)
        {
            _identityServerHost = identityServerHost;
            _apiHost = apiHost;
            _clientId = clientId;
            _useForwardedHeaders = useForwardedHeaders;

            OnConfigureServices += ConfigureServices;
            OnConfigure += Configure;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddAuthorization();

            var bff = services.AddBff(options => { 
                BffOptions = options; 
            });

            services.AddSingleton<IHttpMessageInvokerFactory>(
                new CallbackHttpMessageInvokerFactory(
                    path => new HttpMessageInvoker(_apiHost.Server.CreateHandler())));

            services.AddAuthentication("cookie")
                .AddCookie("cookie", options =>
                {
                    options.Cookie.Name = "bff";
                });

            bff.AddServerSideSessions();
            bff.AddRemoteApis();

            services.AddAuthentication(options =>
                {
                    options.DefaultChallengeScheme = "oidc";
                    options.DefaultSignOutScheme = "oidc";
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Events.OnUserInformationReceived = context =>
                    {
                        StoreNamedTokens((context.ProtocolMessage.AccessToken, context.ProtocolMessage.RefreshToken), context.Properties, context.ProtocolMessage.IdToken);
                        return Task.CompletedTask;
                    };

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

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AlwaysFail", policy => { policy.RequireAssertion(ctx => false); });
            });
        }

        public static void StoreNamedTokens((string accessToken, string refreshToken) userTokens, AuthenticationProperties authenticationProperties, string identityToken = null)
        {
            var tokens = new List<AuthenticationToken>();
            tokens.Add(new AuthenticationToken { Name = $"{OpenIdConnectParameterNames.AccessToken}::named_token_stored", Value = userTokens.accessToken,  });
            tokens.Add(new AuthenticationToken { Name = $"{OpenIdConnectParameterNames.TokenType}::named_token_stored", Value = "Bearer",  });
            authenticationProperties.StoreTokens(tokens);
        }

        private void Configure(IApplicationBuilder app)
        {
            if (_useForwardedHeaders)
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                       ForwardedHeaders.XForwardedProto |
                                       ForwardedHeaders.XForwardedHost
                });
            }

            app.UseAuthentication();

            app.UseRouting();

            app.UseBff();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBffManagementEndpoints();

                endpoints.MapRemoteBffApiEndpoint(
                        "/api_user_with_useraccesstokenparameters_having_stored_named_token", _apiHost.Url())
                    .WithUserAccessTokenParameter(new BffUserAccessTokenParameters("cookie", null, true, "named_token_stored"))
                    .RequireAccessToken();

                endpoints.MapRemoteBffApiEndpoint(
                        "/api_user_with_useraccesstokenparameters_having_not_stored_named_token", _apiHost.Url())
                    .WithUserAccessTokenParameter(new BffUserAccessTokenParameters("cookie", null, true, "named_token_not_stored"))
                    .RequireAccessToken();
            });

            app.Map("/invalid_endpoint",
                invalid => invalid.Use(next => RemoteApiEndpoint.Map("/invalid_endpoint", _apiHost.Url())));
        }

        public async Task<bool> GetIsUserLoggedInAsync(string userQuery = null)
        {
            if (userQuery != null) userQuery = "?" + userQuery;

            var req = new HttpRequestMessage(HttpMethod.Get, Url("/bff/user") + userQuery);
            req.Headers.Add("x-csrf", "1");
            var response = await BrowserClient.SendAsync(req);

            (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized).Should()
                .BeTrue();

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<List<JsonRecord>> CallUserEndpointAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, Url("/bff/user"));
            req.Headers.Add("x-csrf", "1");

            var response = await BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<JsonRecord>>(json);
        }

        public async Task<HttpResponseMessage> BffLoginAsync(string sub, string sid = null)
        {
            await _identityServerHost.CreateIdentityServerSessionCookieAsync(sub, sid);
            return await BffOidcLoginAsync();
        }

        public async Task<HttpResponseMessage> BffOidcLoginAsync()
        {
            var response = await BrowserClient.GetAsync(Url("/bff/login"));
            response.StatusCode.Should().Be(HttpStatusCode.Redirect); // authorize
            response.Headers.Location.ToString().ToLowerInvariant().Should()
                .StartWith(_identityServerHost.Url("/connect/authorize"));

            response = await _identityServerHost.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(HttpStatusCode.Redirect); // client callback
            response.Headers.Location.ToString().ToLowerInvariant().Should().StartWith(Url("/signin-oidc"));

            response = await BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(HttpStatusCode.Redirect); // root
            response.Headers.Location.ToString().ToLowerInvariant().Should().Be("/");

            (await GetIsUserLoggedInAsync()).Should().BeTrue();

            response = await BrowserClient.GetAsync(Url(response.Headers.Location.ToString()));
            return response;
        }

        public async Task<HttpResponseMessage> BffLogoutAsync(string sid = null)
        {
            var response = await BrowserClient.GetAsync(Url("/bff/logout") + "?sid=" + sid);
            response.StatusCode.Should().Be(HttpStatusCode.Redirect); // endsession
            response.Headers.Location.ToString().ToLowerInvariant().Should()
                .StartWith(_identityServerHost.Url("/connect/endsession"));

            response = await _identityServerHost.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(HttpStatusCode.Redirect); // logout
            response.Headers.Location.ToString().ToLowerInvariant().Should()
                .StartWith(_identityServerHost.Url("/account/logout"));

            response = await _identityServerHost.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(HttpStatusCode.Redirect); // post logout redirect uri
            response.Headers.Location.ToString().ToLowerInvariant().Should().StartWith(Url("/signout-callback-oidc"));

            response = await BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(HttpStatusCode.Redirect); // root
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