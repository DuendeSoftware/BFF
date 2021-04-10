using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Duende.Bff.Tests.TestFramework;
using Duende.Bff.Tests.TestHosts;
using FluentAssertions;
using Xunit;

namespace Duende.Bff.Tests.Endpoints
{
    public class ForwardedHeadersTests: BffIntegrationTestBase
    {
        public ForwardedHeadersTests()
        {
            BffHost = new BffHost(IdentityServerHost, ApiHost, "spa", "https://bff.internal", useForwardedHeaders: true);
            BffHost.InitializeAsync().Wait();
        }
        
        [Fact]
        public async Task calls_to_local_endpoint_should_return_standard_host_name()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/local_anon"));
            req.Headers.Add("x-csrf", "1");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);

            var host = apiResult.RequestHeaders["Host"].Single();
            host.Should().Be("bff.internal");
        }
        
        [Fact]
        public async Task calls_to_local_endpoint_should_return_forwarded_host_name()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/local_anon"));
            req.Headers.Add("x-csrf", "1");
            req.Headers.Add("X-Forwarded-Host", "bff.public");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);

            var host = apiResult.RequestHeaders["Host"].Single();
            host.Should().Be("bff.public");
        }
        
        // [Fact]
        // public async Task calls_to_remote_endpoint_should_forward_user_to_api()
        // {
        //     await _bffHost.BffLoginAsync("alice");
        //
        //     var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_user/test"));
        //     req.Headers.Add("x-csrf", "1");
        //     var response = await _bffHost.BrowserClient.SendAsync(req);
        //
        //     response.IsSuccessStatusCode.Should().BeTrue();
        //     response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
        //     var json = await response.Content.ReadAsStringAsync();
        //     var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
        //     apiResult.Method.Should().Be("GET");
        //     apiResult.Path.Should().Be("/test");
        //     apiResult.Sub.Should().Be("alice");
        //     apiResult.ClientId.Should().Be("spa");
        // }
    }
}