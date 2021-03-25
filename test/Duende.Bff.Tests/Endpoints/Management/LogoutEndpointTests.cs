using Duende.Bff.Tests.TestFramework;
using FluentAssertions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.Tests.Endpoints.Management
{
    public class LogoutEndpointTests : BffIntegrationTestBase
    {
        [Fact]
        public async Task logout_endpoint_should_signout()
        {
            await _bffHost.BffLoginAsync("alice", "sid123");
            
            await _bffHost.BffLogoutAsync("sid123");

            (await _bffHost.GetIsUserLoggedInAsync()).Should().BeFalse();
        }

        [Fact(Skip = "need to implement")]
        public async Task logout_endpoint_without_sid_should_fail()
        {
            await _bffHost.BffLoginAsync("alice", "sid123");

            await _bffHost.BffLogoutAsync();

            (await _bffHost.GetIsUserLoggedInAsync()).Should().BeTrue();
        }

        [Fact(Skip = "need to implement")]
        public async Task logout_endpoint_without_session_should_fail()
        {
            await _bffHost.BffLogoutAsync("sid123");

            (await _bffHost.GetIsUserLoggedInAsync()).Should().BeTrue();
        }

        [Fact]
        public async Task logout_endpoint_should_redirect_to_external_signout_and_return_to_root()
        {
            await _bffHost.BffLoginAsync("alice", "sid123");
            
            await _bffHost.BffLogoutAsync("sid123");
            
            _bffHost.BrowserClient.CurrentUri.ToString().ToLowerInvariant().Should().Be(_bffHost.Url("/"));
            (await _bffHost.GetIsUserLoggedInAsync()).Should().BeFalse();
        }

        [Fact]
        public async Task logout_endpoint_should_accept_returnUrl()
        {
            await _bffHost.BffLoginAsync("alice", "sid123");

            var response = await _bffHost.BrowserClient.GetAsync(_bffHost.Url("/bff/logout") + "?sid=sid123&returnUrl=/foo");
            response.StatusCode.Should().Be(302); // endsession
            response.Headers.Location.ToString().ToLowerInvariant().Should().StartWith(_identityServerHost.Url("/connect/endsession"));

            response = await _identityServerHost.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(302); // logout
            response.Headers.Location.ToString().ToLowerInvariant().Should().StartWith(_identityServerHost.Url("/account/logout"));

            response = await _identityServerHost.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(302); // post logout redirect uri
            response.Headers.Location.ToString().ToLowerInvariant().Should().StartWith(_bffHost.Url("/signout-callback-oidc"));

            response = await _bffHost.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(302); // root
            response.Headers.Location.ToString().ToLowerInvariant().Should().Be("/foo");
        }

        [Fact]
        public async Task logout_endpoint_should_reject_non_local_returnUrl()
        {
            await _bffHost.BffLoginAsync("alice", "sid123");

            Func<Task> f = () => _bffHost.BrowserClient.GetAsync(_bffHost.Url("/bff/logout") + "?sid=sid123&returnUrl=https://foo");
            f.Should().Throw<Exception>().And.Message.Should().Contain("returnUrl");
        }
    }
}
