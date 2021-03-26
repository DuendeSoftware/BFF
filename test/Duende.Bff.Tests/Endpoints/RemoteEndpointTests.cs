using Duende.Bff.Tests.TestFramework;
using Duende.Bff.Tests.TestHosts;
using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.Tests.Endpoints
{
    public class RemoteEndpointTests : BffIntegrationTestBase
    {
        [Fact]
        public async Task unauthenticated_calls_to_remote_endpoint_should_return_401()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_user/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task calls_to_remote_endpoint_should_forward_user_to_api()
        {
            await _bffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_user/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.method.Should().Be("GET");
            apiResult.path.Should().Be("/test");
            apiResult.sub.Should().Be("alice");
            apiResult.clientId.Should().Be("spa");
        }

        [Fact]
        public async Task put_to_remote_endpoint_should_forward_user_to_api()
        {
            await _bffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Put, _bffHost.Url("/api_user/test"));
            req.Headers.Add("x-csrf", "1");
            req.Content = new StringContent(JsonSerializer.Serialize(new TestPayload("hello test api")), Encoding.UTF8, "application/json");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.method.Should().Be("PUT");
            apiResult.path.Should().Be("/test");
            apiResult.sub.Should().Be("alice");
            apiResult.clientId.Should().Be("spa");
            var body = JsonSerializer.Deserialize<TestPayload>(apiResult.body);
            body.message.Should().Be("hello test api");
        }



        [Fact]
        public async Task calls_to_remote_endpoint_should_forward_user_or_anonymous_to_api()
        {
            {
                var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_user_or_anon/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await _bffHost.BrowserClient.SendAsync(req);

                response.IsSuccessStatusCode.Should().BeTrue();
                response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
                var json = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
                apiResult.method.Should().Be("GET");
                apiResult.path.Should().Be("/test");
                apiResult.sub.Should().BeNull();
                apiResult.clientId.Should().BeNull();
            }

            {
                await _bffHost.BffLoginAsync("alice");

                var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_user_or_anon/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await _bffHost.BrowserClient.SendAsync(req);

                response.IsSuccessStatusCode.Should().BeTrue();
                response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
                var json = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
                apiResult.method.Should().Be("GET");
                apiResult.path.Should().Be("/test");
                apiResult.sub.Should().Be("alice");
                apiResult.clientId.Should().Be("spa");
            }
        }

        [Fact]
        public async Task calls_to_remote_endpoint_should_forward_client_token_to_api()
        {
            await _bffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_client/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.method.Should().Be("GET");
            apiResult.path.Should().Be("/test");
            apiResult.sub.Should().BeNull();
            apiResult.clientId.Should().Be("spa");
        }
        
        [Fact]
        public async Task calls_to_remote_endpoint_should_forward_user_or_client_to_api()
        {
            {
                var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_user_or_client/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await _bffHost.BrowserClient.SendAsync(req);

                response.IsSuccessStatusCode.Should().BeTrue();
                response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
                var json = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
                apiResult.method.Should().Be("GET");
                apiResult.path.Should().Be("/test");
                apiResult.sub.Should().BeNull();
                apiResult.clientId.Should().Be("spa");
            }

            {
                await _bffHost.BffLoginAsync("alice");

                var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_user_or_client/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await _bffHost.BrowserClient.SendAsync(req);

                response.IsSuccessStatusCode.Should().BeTrue();
                response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
                var json = await response.Content.ReadAsStringAsync();
                var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
                apiResult.method.Should().Be("GET");
                apiResult.path.Should().Be("/test");
                apiResult.sub.Should().Be("alice");
                apiResult.clientId.Should().Be("spa");
            }
        }

        [Fact(Skip = "Unclear: Is RequireAccessToken optional?")]
        public async Task calls_to_remote_endpoint_with_no_token_type_configured_should_fail()
        {
            {
                var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_user_notokentype/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await _bffHost.BrowserClient.SendAsync(req);

                response.IsSuccessStatusCode.Should().BeFalse();
            }

            {
                await _bffHost.BffLoginAsync("alice");

                var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_user_notokentype/test"));
                req.Headers.Add("x-csrf", "1");
                var response = await _bffHost.BrowserClient.SendAsync(req);

                response.IsSuccessStatusCode.Should().BeFalse();
            }
        }



        [Fact]
        public async Task response_status_401_from_remote_endpoint_should_return_401_from_bff()
        {
            await _bffHost.BffLoginAsync("alice");
            _apiHost.ApiStatusCodeToReturn = 401;

            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_user/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        
        [Fact]
        public async Task response_status_403_from_remote_endpoint_should_return_403_from_bff()
        {
            await _bffHost.BffLoginAsync("alice");
            _apiHost.ApiStatusCodeToReturn = 403;

            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_user/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }



        [Fact]
        public async Task calls_to_remote_endpoint_should_require_csrf()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_user_or_client/test"));
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task endpoints_that_disable_csrf_should_not_require_csrf_header()
        {
            await _bffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/api_user_no_csrf/test"));
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.method.Should().Be("GET");
            apiResult.path.Should().Be("/test");
            apiResult.sub.Should().Be("alice");
            apiResult.clientId.Should().Be("spa");
        }
    }
}
