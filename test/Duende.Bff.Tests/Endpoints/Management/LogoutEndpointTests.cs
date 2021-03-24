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
        private readonly TestHost _host;
        
        MockExternalAuthenticationHandler _mockExternalAuthenticationHandler;

        public LogoutEndpointTests()
        {
            _host = new TestHost();
            _host.OnConfigureServices += ConfigureServices;
            _host.OnConfigure += Configure;
            _host.InitializeAsync().Wait();
            _mockExternalAuthenticationHandler = _host.Resolve<MockExternalAuthenticationHandler>();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddBff();
            services.AddAuthentication(options=> 
            {
                options.DefaultScheme = "cookie";
                options.DefaultChallengeScheme = "external";
                options.DefaultSignOutScheme = "external";
            })
                .AddCookie("cookie")
                .AddMockExternalAuthentication("external");
            services.AddSingleton<MockExternalAuthenticationHandler>();
        }
        
        private void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseRouting();
            
            app.UseEndpoints(endpoints => 
            {
                endpoints.MapBffManagementEndpoints();
            });
        }

        [Fact]
        public async Task logout_endpoint_should_signout()
        {
            await _host.SignInAsync(new Claim("sub", "alice"), new Claim("sid", "sid123"));

            await _host.BrowserClient.GetAsync(_host.Url("/bff/logout") + "?sid=sid123");

            (await _host.GetIsUserLoggedInAsync()).Should().BeFalse();
        }

        [Fact(Skip = "need to implement")]
        public async Task logout_endpoint_without_sid_should_fail()
        {
            await _host.SignInAsync(new Claim("sub", "alice"), new Claim("sid", "sid123"));

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
            await _host.SignInAsync(new Claim("sub", "alice"), new Claim("sid", "sid123"));

            await _host.BrowserClient.GetAsync(_host.Url("/bff/logout") + "?sid=sid123");

            _mockExternalAuthenticationHandler.SignOutWasCalled.Should().BeTrue();
            _mockExternalAuthenticationHandler.SignOutAuthenticationProperties.RedirectUri.Should().Be("/");
        }

        [Fact]
        public async Task logout_endpoint_should_accept_returnUrl()
        {
            await _host.SignInAsync(new Claim("sub", "alice"), new Claim("sid", "sid123"));

            await _host.BrowserClient.GetAsync(_host.Url("/bff/logout") + "?sid=sid123&returnUrl=/foo");

            _mockExternalAuthenticationHandler.SignOutAuthenticationProperties.RedirectUri.Should().Be("/foo");
        }
        
        [Fact]
        public async Task logout_endpoint_should_reject_non_local_returnUrl()
        {
            await _host.SignInAsync(new Claim("sub", "alice"), new Claim("sid", "sid123"));

            Func<Task> f = () => _host.BrowserClient.GetAsync(_host.Url("/bff/logout") + "?sid=sid123&returnUrl=https:///foo");
            f.Should().Throw<Exception>().And.Message.Should().Contain("returnUrl");
        }
    }
}
