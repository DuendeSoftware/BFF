using Duende.Bff.Tests.TestFramework;
using FluentAssertions;
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
            await _bffHost.BffLoginAsync("alice");
            (await _bffHost.GetIsUserLoggedInAsync()).Should().BeTrue();

            await _identityServerHost.RevokeSessionCookieAsync();

            (await _bffHost.GetIsUserLoggedInAsync()).Should().BeFalse();
        }

    }
}
