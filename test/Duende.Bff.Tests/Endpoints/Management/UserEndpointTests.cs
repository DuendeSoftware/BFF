// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Linq;
using Duende.Bff.Tests.TestFramework;
using Duende.Bff.Tests.TestHosts;
using FluentAssertions;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.Tests.Endpoints.Management
{
    public class UserEndpointTests : BffIntegrationTestBase
    {
        [Fact]
        public async Task user_endpoint_for_authenticated_user_should_return_claims()
        {
            await BffHost.IssueSessionCookieAsync(new Claim("sub", "alice"), new Claim("foo", "foo1"), new Claim("foo", "foo2"));

            var claims = await BffHost.GetUserClaimsAsync();

            claims.Length.Should().Be(4);
            claims.Should().Contain(new ClaimRecord("sub", "alice"));
            claims.Should().Contain(new ClaimRecord("foo", "foo1"));
            claims.Should().Contain(new ClaimRecord("foo", "foo2"));
            claims.Select(c => c.type).Should().Contain("bff:session_expires_in");
        }
        
        [Fact]
        public async Task user_endpoint_for_authenticated_user_with_sid_should_return_claims_including_logout()
        {
            await BffHost.IssueSessionCookieAsync(
                new Claim("sub", "alice"), 
                new Claim("foo", "foo1"), 
                new Claim("foo", "foo2"),
                new Claim("sid", "123"));

            var claims = await BffHost.GetUserClaimsAsync();

            claims.Length.Should().Be(6);
            claims.Should().Contain(new ClaimRecord("sub", "alice"));
            claims.Should().Contain(new ClaimRecord("foo", "foo1"));
            claims.Should().Contain(new ClaimRecord("foo", "foo2"));
            claims.Should().Contain(new ClaimRecord("sid", "123"));
            claims.Should().Contain(new ClaimRecord(Constants.ClaimTypes.LogoutUrl, "/bff/logout?sid=123"));
            claims.Select(c => c.type).Should().Contain(Constants.ClaimTypes.SessionExpiresIn);
        }

        [Fact]
        public async Task user_endpoint_for_authenticated_user_without_csrf_header_should_fail()
        {
            await BffHost.IssueSessionCookieAsync(new Claim("sub", "alice"), new Claim("foo", "foo1"), new Claim("foo", "foo2"));

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/bff/user"));
            var response = await BffHost.BrowserClient.SendAsync(req);
            
            response.StatusCode.Should().Be(401);
        }
        
        [Fact]
        public async Task user_endpoint_for_unauthenticated_user_should_fail()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/bff/user"));
            req.Headers.Add("x-csrf", "1");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(401);
        }

    }
}
