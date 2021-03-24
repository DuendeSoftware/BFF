using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Duende.Bff.Tests.TestFramework
{
    public class BffIntegrationTestBase
    {
        protected readonly IdentityServerHost _isHost;
        protected readonly ApiHost _apiHost;
        protected readonly BffHost _host;

        public BffIntegrationTestBase()
        {
            _isHost = new IdentityServerHost();
            _isHost.Clients.Add(new Client
            {
                ClientId = "spa",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                RedirectUris = { "https://app/signin-oidc" },
                PostLogoutRedirectUris = { "https://app/signout-callback-oidc" },
                BackChannelLogoutUri = "https://app/bff/backchannel",
                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "scope1" }
            });
            _isHost.OnConfigureServices += services => {
                services.AddTransient<IBackChannelLogoutHttpClient>(provider => 
                    new DefaultBackChannelLogoutHttpClient(_host.HttpClient, provider.GetRequiredService<ILoggerFactory>()));
            };
            _isHost.InitializeAsync().Wait();

            _apiHost = new ApiHost(_isHost, "scope1");
            _apiHost.InitializeAsync(_isHost.BrowserClient.CookieContainer).Wait();

            _host = new BffHost(_isHost, _apiHost, "spa");
            _host.InitializeAsync(_isHost.BrowserClient.CookieContainer).Wait();
        }

        public async Task Login(string sub)
        {
            await _isHost.IssueSessionCookieAsync(new Claim("sub", sub));
        }
    }
}
