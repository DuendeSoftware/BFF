// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff.Tests.TestHosts;
using FluentAssertions;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.Tests.Endpoints.Management
{
    public class LoginEndpointTests : BffIntegrationTestBase
    {
        [Fact]
        public async Task login_endpoint_should_challenge_and_redirect_to_root()
        {
            var response = await _bffHost.BrowserClient.GetAsync(_bffHost.Url("/bff/login"));
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith(_identityServerHost.Url("/connect/authorize"));

            await _identityServerHost.IssueSessionCookieAsync("alice");
            response = await _identityServerHost.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith(_bffHost.Url("/signin-oidc"));

            response = await _bffHost.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().Be("/");
        }

        [Fact]
        public async Task login_endpoint_with_existing_session_should_challenge()
        {
            await _bffHost.BffLoginAsync("alice");

            var response = await _bffHost.BrowserClient.GetAsync(_bffHost.Url("/bff/login"));
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith(_identityServerHost.Url("/connect/authorize"));
        }

        [Fact]
        public async Task login_endpoint_should_accept_returnUrl()
        {
            var response = await _bffHost.BrowserClient.GetAsync(_bffHost.Url("/bff/login") + "?returnUrl=/foo");
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith(_identityServerHost.Url("/connect/authorize"));

            await _identityServerHost.IssueSessionCookieAsync("alice");
            response = await _identityServerHost.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().StartWith(_bffHost.Url("/signin-oidc"));

            response = await _bffHost.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().Should().Be("/foo");
        }

        [Fact]
        public void login_endpoint_should_not_accept_non_local_returnUrl()
        {
            Func<Task> f = () => _bffHost.BrowserClient.GetAsync(_bffHost.Url("/bff/login") + "?returnUrl=https://foo");
            f.Should().Throw<Exception>().And.Message.Should().Contain("returnUrl");
        }
    }
}
