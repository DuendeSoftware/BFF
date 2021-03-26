using Duende.Bff.Tests.TestFramework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.Tests.Endpoints
{
    public class RemoteEndpointTests : BffIntegrationTestBase
    {
        [Fact]
        public async Task calls_to_remote_endpoint_should_forward_to_api()
        {
            await _bffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.path.Should().Be("/test");
            apiResult.sub.Should().Be("alice");
        }

    }
}
