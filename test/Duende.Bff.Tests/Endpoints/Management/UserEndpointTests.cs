using Duende.Bff.Tests.TestFramework;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.Tests.Endpoints.Management
{
    public class UserEndpointTests
    {
        private readonly TestHost _host;

        public UserEndpointTests()
        {
            _host = new TestHost();
            _host.OnConfigureServices += ConfigureServices;
            _host.OnConfigure += Configure;
            _host.InitializeAsync().Wait();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddBff();
            services.AddAuthentication("cookie")
                .AddCookie("cookie");
        }
        
        private void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseRouting();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints => 
            {
                endpoints.MapBffManagementEndpoints();
            });
        }

        [Fact]
        public async Task user_endpoint_for_authenticated_user_should_return_claims()
        {
            await _host.SignInAsync(new Claim("sub", "alice"), new Claim("foo", "foo1"), new Claim("foo", "foo2"));

            var req = new HttpRequestMessage(HttpMethod.Get, _host.Url("/bff/user"));
            req.Headers.Add("x-csrf", "1");
            var response = await _host.BrowserClient.SendAsync(req);

            var claims = await response.ReadUserClaimsAsync();

            claims.Length.Should().Be(3);
            claims.Should().Contain(new ClaimRecord("sub", "alice"));
            claims.Should().Contain(new ClaimRecord("foo", "foo1"));
            claims.Should().Contain(new ClaimRecord("foo", "foo2"));
        }

        [Fact]
        public async Task user_endpoint_for_authenticated_user_without_csrf_header_should_fail()
        {
            await _host.SignInAsync(new Claim("sub", "alice"), new Claim("foo", "foo1"), new Claim("foo", "foo2"));

            var req = new HttpRequestMessage(HttpMethod.Get, _host.Url("/bff/user"));
            var response = await _host.BrowserClient.SendAsync(req);
            
            response.StatusCode.Should().Be(401);
        }
        
        [Fact]
        public async Task user_endpoint_for_unauthenticated_user_should_fail()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, _host.Url("/bff/user"));
            req.Headers.Add("x-csrf", "1");
            var response = await _host.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(401);
        }

    }
}
