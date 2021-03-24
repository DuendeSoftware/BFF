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
    public class LogoutEndpointTests
    {
        private readonly BffHost _host;
        
        MockExternalAuthenticationHandler _mockExternalAuthenticationHandler;

        public LogoutEndpointTests()
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
                options.DefaultSignOutScheme = "external";
            })
                .AddMockExternalAuthentication("external");
            services.AddSingleton<MockExternalAuthenticationHandler>();
        }
        
        [Fact]
        public async Task logout_endpoint_should_signout()
        {
            await _host.IssueSessionCookieAsync(new Claim("sub", "alice"), new Claim("sid", "sid123"));

            await _host.BrowserClient.GetAsync(_host.Url("/bff/logout") + "?sid=sid123");

            (await _host.GetIsUserLoggedInAsync()).Should().BeFalse();
        }

        [Fact(Skip = "need to implement")]
        public async Task logout_endpoint_without_sid_should_fail()
        {
            await _host.IssueSessionCookieAsync(new Claim("sub", "alice"), new Claim("sid", "sid123"));

            await _host.BrowserClient.GetAsync(_host.Url("/bff/logout"));

            (await _host.GetIsUserLoggedInAsync()).Should().BeTrue();
            _mockExternalAuthenticationHandler.SignOutWasCalled.Should().BeFalse();
        }

        [Fact(Skip = "need to implement")]
        public async Task logout_endpoint_without_session_should_fail()
        {
            await _host.BrowserClient.GetAsync(_host.Url("/bff/logout"));

            _mockExternalAuthenticationHandler.SignOutWasCalled.Should().BeFalse();
        }

        [Fact]
        public async Task logout_endpoint_should_redirect_to_external_signout_and_return_to_root()
        {
            await _host.IssueSessionCookieAsync(new Claim("sub", "alice"), new Claim("sid", "sid123"));

            await _host.BrowserClient.GetAsync(_host.Url("/bff/logout") + "?sid=sid123");

            _mockExternalAuthenticationHandler.SignOutWasCalled.Should().BeTrue();
            _mockExternalAuthenticationHandler.SignOutAuthenticationProperties.RedirectUri.Should().Be("/");
        }

        [Fact]
        public async Task logout_endpoint_should_accept_returnUrl()
        {
            await _host.IssueSessionCookieAsync(new Claim("sub", "alice"), new Claim("sid", "sid123"));

            await _host.BrowserClient.GetAsync(_host.Url("/bff/logout") + "?sid=sid123&returnUrl=/foo");

            _mockExternalAuthenticationHandler.SignOutAuthenticationProperties.RedirectUri.Should().Be("/foo");
        }
        
        [Fact]
        public async Task logout_endpoint_should_reject_non_local_returnUrl()
        {
            await _host.IssueSessionCookieAsync(new Claim("sub", "alice"), new Claim("sid", "sid123"));

            Func<Task> f = () => _host.BrowserClient.GetAsync(_host.Url("/bff/logout") + "?sid=sid123&returnUrl=https:///foo");
            f.Should().Throw<Exception>().And.Message.Should().Contain("returnUrl");
        }
    }
}
