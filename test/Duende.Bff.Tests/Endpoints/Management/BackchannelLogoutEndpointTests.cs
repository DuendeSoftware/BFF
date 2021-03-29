// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.Bff.Tests.TestFramework;
using Duende.Bff.Tests.TestHosts;
using FluentAssertions;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.Tests.Endpoints.Management
{
    public class BackchannelLogoutEndpointTests : BffIntegrationTestBase
    {
        [Fact]
        public async Task backchannel_logout_endpoint_should_signout()
        {
            await _bffHost.BffLoginAsync("alice", "sid123");

            await _identityServerHost.RevokeSessionCookieAsync();

            (await _bffHost.GetIsUserLoggedInAsync()).Should().BeFalse();
        }

        [Fact]
        public async Task backchannel_logout_endpoint_for_incorrect_sub_should_not_logout_user()
        {
            await _bffHost.BffLoginAsync("alice", "sid123");

            await _identityServerHost.CreateIdentityServerSessionCookieAsync("bob", "sid123");

            await _identityServerHost.RevokeSessionCookieAsync();

            (await _bffHost.GetIsUserLoggedInAsync()).Should().BeTrue();
        }

        [Fact]
        public async Task backchannel_logout_endpoint_for_incorrect_sid_should_not_logout_user()
        {
            await _bffHost.BffLoginAsync("alice", "sid123");

            await _identityServerHost.CreateIdentityServerSessionCookieAsync("alice", "sid999");

            await _identityServerHost.RevokeSessionCookieAsync();

            (await _bffHost.GetIsUserLoggedInAsync()).Should().BeTrue();
        }

    }
}
