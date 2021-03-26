using Duende.Bff.Tests.TestHosts;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Duende.Bff.Tests.TestHosts
{
    public class BffIntegrationTestBase
    {
        protected readonly IdentityServerHost _identityServerHost;
        protected readonly ApiHost _apiHost;
        protected readonly BffHost _bffHost;

        public BffIntegrationTestBase()
        {
            _identityServerHost = new IdentityServerHost();
            _identityServerHost.Clients.Add(new Client
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
            _identityServerHost.OnConfigureServices += services => {
                services.AddTransient<IBackChannelLogoutHttpClient>(provider => 
                    new DefaultBackChannelLogoutHttpClient(_bffHost.HttpClient, provider.GetRequiredService<ILoggerFactory>()));
            };
            _identityServerHost.InitializeAsync().Wait();

            _apiHost = new ApiHost(_identityServerHost, "scope1");
            _apiHost.InitializeAsync().Wait();

            _bffHost = new BffHost(_identityServerHost, _apiHost, "spa");
            _bffHost.InitializeAsync().Wait();
        }

        public async Task Login(string sub)
        {
            await _identityServerHost.IssueSessionCookieAsync(new Claim("sub", sub));
        }
    }
}
