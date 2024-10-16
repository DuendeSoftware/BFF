﻿// Copyright (c) Duende Software. All rights reserved.
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
using Duende.Bff.Yarp;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Authentication;

namespace Duende.Bff.Tests.TestHosts;

public class BffHost : GenericHost
{
    public enum ResponseStatus
    {
        Ok, Challenge, Forbid
    }
    public ResponseStatus LocalApiResponseStatus { get; set; } = ResponseStatus.Ok;

    private readonly IdentityServerHost _identityServerHost;
    private readonly ApiHost _apiHost;
    private readonly string _clientId;
    private readonly bool _useForwardedHeaders;

    public BffOptions BffOptions { get; private set; }

    public BffHost(IdentityServerHost identityServerHost, ApiHost apiHost, string clientId,
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

        var bff = services.AddBff(options =>
        {
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

        services.AddSingleton<FailureAccessTokenRetriever>();

        services.AddSingleton(new TestAccessTokenRetriever(async ()
            => await _identityServerHost.CreateJwtAccessTokenAsync()));
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

            endpoints.Map("/local_anon", async context =>
                {
                    // capture body if present
                    var body = default(string);
                    if (context.Request.HasJsonContentType())
                    {
                        using (var sr = new StreamReader(context.Request.Body))
                        {
                            body = await sr.ReadToEndAsync();
                        }
                    }

                    // capture request headers
                    var requestHeaders = new Dictionary<string, List<string>>();
                    foreach (var header in context.Request.Headers)
                    {
                        var values = new List<string>(header.Value.Select(v => v));
                        requestHeaders.Add(header.Key, values);
                    }

                    var response = new ApiResponse(
                        context.Request.Method,
                        context.Request.Path.Value,
                        context.User.FindFirst("sub")?.Value,
                        context.User.FindFirst("client_id")?.Value,
                        context.User.Claims.Select(x => new ClaimRecord(x.Type, x.Value)).ToArray())
                    {
                        Body = body,
                        RequestHeaders = requestHeaders
                    };

                    if (LocalApiResponseStatus == ResponseStatus.Ok)
                    {
                        context.Response.StatusCode = 200;

                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    }
                    else if (LocalApiResponseStatus == ResponseStatus.Challenge)
                    {
                        await context.ChallengeAsync();
                    }
                    else if (LocalApiResponseStatus == ResponseStatus.Forbid)
                    {
                        await context.ForbidAsync();
                    }
                    else
                    {
                        throw new Exception("Invalid LocalApiResponseStatus");
                    }
                })
                .AsBffApiEndpoint();

            endpoints.Map("/local_anon_no_csrf", async context =>
                {
                    // capture body if present
                    var body = default(string);
                    if (context.Request.HasJsonContentType())
                    {
                        using (var sr = new StreamReader(context.Request.Body))
                        {
                            body = await sr.ReadToEndAsync();
                        }
                    }

                    // capture request headers
                    var requestHeaders = new Dictionary<string, List<string>>();
                    foreach (var header in context.Request.Headers)
                    {
                        var values = new List<string>(header.Value.Select(v => v));
                        requestHeaders.Add(header.Key, values);
                    }

                    var response = new ApiResponse(
                        context.Request.Method,
                        context.Request.Path.Value,
                        context.User.FindFirst("sub")?.Value,
                        context.User.FindFirst("client_id")?.Value,
                        context.User.Claims.Select(x => new ClaimRecord(x.Type, x.Value)).ToArray())
                    {
                        Body = body,
                        RequestHeaders = requestHeaders
                    };

                    if (LocalApiResponseStatus == ResponseStatus.Ok)
                    {
                        context.Response.StatusCode = 200;

                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    }
                    else if (LocalApiResponseStatus == ResponseStatus.Challenge)
                    {
                        await context.ChallengeAsync();
                    }
                    else if (LocalApiResponseStatus == ResponseStatus.Forbid)
                    {
                        await context.ForbidAsync();
                    }
                    else
                    {
                        throw new Exception("Invalid LocalApiResponseStatus");
                    }
                })
                .AsBffApiEndpoint()
                .SkipAntiforgery();

            endpoints.Map("/local_anon_no_csrf_no_response_handling", async context =>
            {
                // capture body if present
                var body = default(string);
                if (context.Request.HasJsonContentType())
                {
                    using (var sr = new StreamReader(context.Request.Body))
                    {
                        body = await sr.ReadToEndAsync();
                    }
                }

                // capture request headers
                var requestHeaders = new Dictionary<string, List<string>>();
                foreach (var header in context.Request.Headers)
                {
                    var values = new List<string>(header.Value.Select(v => v));
                    requestHeaders.Add(header.Key, values);
                }

                var response = new ApiResponse(
                    context.Request.Method,
                    context.Request.Path.Value,
                    context.User.FindFirst("sub")?.Value,
                    context.User.FindFirst("client_id")?.Value,
                    context.User.Claims.Select(x => new ClaimRecord(x.Type, x.Value)).ToArray())
                {
                    Body = body,
                    RequestHeaders = requestHeaders
                };

                if (LocalApiResponseStatus == ResponseStatus.Ok)
                {
                    context.Response.StatusCode = 200;

                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                }
                else if (LocalApiResponseStatus == ResponseStatus.Challenge)
                {
                    await context.ChallengeAsync();
                }
                else if (LocalApiResponseStatus == ResponseStatus.Forbid)
                {
                    await context.ForbidAsync();
                }
                else
                {
                    throw new Exception("Invalid LocalApiResponseStatus");
                }
            })
            .AsBffApiEndpoint()
            .SkipAntiforgery()
            .SkipResponseHandling();


            endpoints.Map("/local_authz", async context =>
                {
                    var sub = context.User.FindFirst("sub")?.Value;
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
                        context.User.FindFirst("client_id")?.Value,
                        context.User.Claims.Select(x => new ClaimRecord(x.Type, x.Value)).ToArray())
                    {
                        Body = body
                    };

                    if (LocalApiResponseStatus == ResponseStatus.Ok)
                    {
                        context.Response.StatusCode = 200;

                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    }
                    else if (LocalApiResponseStatus == ResponseStatus.Challenge)
                    {
                        await context.ChallengeAsync();
                    }
                    else if (LocalApiResponseStatus == ResponseStatus.Forbid)
                    {
                        await context.ForbidAsync();
                    }
                    else
                    {
                        throw new Exception("Invalid LocalApiResponseStatus");
                    }
                })
                .RequireAuthorization()
                .AsBffApiEndpoint();

            endpoints.Map("/local_authz_no_csrf", async context =>
                {
                    var sub = context.User.FindFirst("sub")?.Value;
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
                        context.User.FindFirst("client_id")?.Value,
                        context.User.Claims.Select(x => new ClaimRecord(x.Type, x.Value)).ToArray())
                    {
                        Body = body
                    };

                    if (LocalApiResponseStatus == ResponseStatus.Ok)
                    {
                        context.Response.StatusCode = 200;

                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    }
                    else if (LocalApiResponseStatus == ResponseStatus.Challenge)
                    {
                        await context.ChallengeAsync();
                    }
                    else if (LocalApiResponseStatus == ResponseStatus.Forbid)
                    {
                        await context.ForbidAsync();
                    }
                    else
                    {
                        throw new Exception("Invalid LocalApiResponseStatus");
                    }
                })
                .RequireAuthorization()
                .AsBffApiEndpoint()
                .SkipAntiforgery();


            endpoints.Map("/always_fail_authz_non_bff_endpoint", context => { return Task.CompletedTask; })
                .RequireAuthorization("AlwaysFail");

            endpoints.Map("/always_fail_authz", context => { return Task.CompletedTask; })
                .AsBffApiEndpoint()
                .RequireAuthorization("AlwaysFail");

            endpoints.MapRemoteBffApiEndpoint(
                    "/api_user", _apiHost.Url())
                .RequireAccessToken();

            endpoints.MapRemoteBffApiEndpoint(
                    "/api_user_no_csrf", _apiHost.Url())
                .SkipAntiforgery()
                .RequireAccessToken();

            endpoints.MapRemoteBffApiEndpoint(
                    "/api_client", _apiHost.Url())
                .RequireAccessToken(TokenType.Client);

            endpoints.MapRemoteBffApiEndpoint(
                    "/api_user_or_client", _apiHost.Url())
                .RequireAccessToken(TokenType.UserOrClient);

            endpoints.MapRemoteBffApiEndpoint(
                    "/api_user_or_anon", _apiHost.Url())
                .WithOptionalUserAccessToken();

            endpoints.MapRemoteBffApiEndpoint(
                "/api_anon_only", _apiHost.Url());

            endpoints.MapRemoteBffApiEndpoint(
                    "/api_with_access_token_retriever", _apiHost.Url())
                .RequireAccessToken(TokenType.UserOrClient)
                .WithAccessTokenRetriever<TestAccessTokenRetriever>();

            endpoints.MapRemoteBffApiEndpoint(
                    "/api_with_access_token_retrieval_that_fails", _apiHost.Url())
                .RequireAccessToken(TokenType.UserOrClient)
                .WithAccessTokenRetriever<FailureAccessTokenRetriever>();

            endpoints.Map(
                "/not_bff_endpoint",
                RemoteApiEndpoint.Map("/not_bff_endpoint", _apiHost.Url()));
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