using Duende.Bff.Tests.TestFramework;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.Tests.Endpoints.Management
{
    public class LoginEndpointTests
    {
        private readonly BffHost _host;
        
        MockExternalAuthenticationHandler _mockExternalAuthenticationHandler;

        public LoginEndpointTests()
        {
            _host = new BffHost();
            _host.OnConfigureServices += ConfigureServices;
            _host.InitializeAsync().Wait();
            _mockExternalAuthenticationHandler = _host.Resolve<MockExternalAuthenticationHandler>();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options=> 
            {
                options.DefaultChallengeScheme = "external";
            })
                .AddMockExternalAuthentication("external");
            services.AddSingleton<MockExternalAuthenticationHandler>();
        }
        

        [Fact]
        public async Task login_endpoint_should_challenge_and_redirect_to_root()
        {
            await _host.BrowserClient.GetAsync(_host.Url("/bff/login"));
            _mockExternalAuthenticationHandler.ChallengeWasCalled.Should().BeTrue();
            _mockExternalAuthenticationHandler.ChallengeAuthenticationProperties.RedirectUri.Should().Be("/");
        }
        
        [Fact]
        public async Task login_endpoint_with_existing_session_should_challenge()
        {
            await _host.IssueSessionCookieAsync(new Claim("sub", "alice"));

            await _host.BrowserClient.GetAsync(_host.Url("/bff/login"));
            _mockExternalAuthenticationHandler.ChallengeWasCalled.Should().BeTrue();
        }

        [Fact]
        public async Task login_endpoint_should_accept_returnUrl()
        {
            await _host.BrowserClient.GetAsync(_host.Url("/bff/login") + "?returnUrl=/foo");
            _mockExternalAuthenticationHandler.ChallengeAuthenticationProperties.RedirectUri.Should().Be("/foo");
        }

        [Fact]
        public void login_endpoint_should_not_accept_non_local_returnUrl()
        {
            Func<Task> f = () => _host.BrowserClient.GetAsync(_host.Url("/bff/login") + "?returnUrl=https://foo");
            f.Should().Throw<Exception>().And.Message.Should().Contain("returnUrl");
        }
    }
}
