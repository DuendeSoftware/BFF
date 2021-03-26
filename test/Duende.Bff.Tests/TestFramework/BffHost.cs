using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Duende.Bff.Tests.TestFramework
{
    public record ClaimRecord(string type, string value);

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

    public class BffHost : GenericHost
    {
        private readonly IdentityServerHost _identityServerHost;
        private readonly ApiHost _apiHost;
        private readonly string _clientId;

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
                
                endpoints.Map("/local", async context =>
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
                    .RequireAuthorization()
                    .AsLocalBffApiEndpoint();

                endpoints.MapRemoteBffApiEndpoint("/api", _apiHost.Url())
                    .RequireAccessToken();
            });
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

    }
}
