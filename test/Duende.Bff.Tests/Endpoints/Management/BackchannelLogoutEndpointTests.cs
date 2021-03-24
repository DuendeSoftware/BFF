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
    public class BackchannelLogoutEndpointTests : BffIntegrationTestBase
    {
        public BackchannelLogoutEndpointTests()
        {
        }

        [Fact]
        public async Task backchannellogout_endpoint_should_signout()
        {
            await _host.BffLoginInAsync("alice");
            (await _host.GetIsUserLoggedInAsync()).Should().BeTrue();

            await _isHost.RevokeSessionCookieAsync();

            (await _host.GetIsUserLoggedInAsync()).Should().BeFalse();
        }

    }
}
