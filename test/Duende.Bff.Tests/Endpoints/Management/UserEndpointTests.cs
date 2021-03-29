// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff.Tests.TestFramework;
using Duende.Bff.Tests.TestHosts;
using FluentAssertions;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.Tests.Endpoints.Management
{
    public class UserEndpointTests : BffIntegrationTestBase
    {
        [Fact]
        public async Task user_endpoint_for_authenticated_user_should_return_claims()
        {
            await _bffHost.IssueSessionCookieAsync(new Claim("sub", "alice"), new Claim("foo", "foo1"), new Claim("foo", "foo2"));

            var claims = await _bffHost.GetUserClaimsAsync();

            claims.Length.Should().Be(3);
            claims.Should().Contain(new ClaimRecord("sub", "alice"));
            claims.Should().Contain(new ClaimRecord("foo", "foo1"));
            claims.Should().Contain(new ClaimRecord("foo", "foo2"));
        }

        [Fact]
        public async Task user_endpoint_for_authenticated_user_without_csrf_header_should_fail()
        {
            await _bffHost.IssueSessionCookieAsync(new Claim("sub", "alice"), new Claim("foo", "foo1"), new Claim("foo", "foo2"));

            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/bff/user"));
            var response = await _bffHost.BrowserClient.SendAsync(req);
            
            response.StatusCode.Should().Be(401);
        }
        
        [Fact]
        public async Task user_endpoint_for_unauthenticated_user_should_fail()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/bff/user"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(401);
        }

    }
}
