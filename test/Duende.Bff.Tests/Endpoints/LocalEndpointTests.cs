// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

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
    public class LocalEndpointTests : BffIntegrationTestBase
    {
        [Fact]
        public async Task calls_to_local_endpoint_should_succeed()
        {
            await _bffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/local_authz"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.method.Should().Be("GET");
            apiResult.path.Should().Be("/local_authz");
            apiResult.sub.Should().Be("alice");
        }

        [Fact]
        public async Task calls_to_local_endpoint_should_require_csrf()
        {
            await _bffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/local_anon"));
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task calls_to_anon_endpoint_should_allow_anonymous()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/local_anon"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.method.Should().Be("GET");
            apiResult.path.Should().Be("/local_anon");
            apiResult.sub.Should().BeNull();
        }

        [Fact]
        public async Task put_to_local_endpoint_should_succeed()
        {
            await _bffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Put, _bffHost.Url("/local_authz"));
            req.Headers.Add("x-csrf", "1");
            req.Content = new StringContent(JsonSerializer.Serialize(new TestPayload("hello test api")), Encoding.UTF8, "application/json");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.method.Should().Be("PUT");
            apiResult.path.Should().Be("/local_authz");
            apiResult.sub.Should().Be("alice");
            var body = JsonSerializer.Deserialize<TestPayload>(apiResult.body);
            body.message.Should().Be("hello test api");
        }

        [Fact]
        public async Task unauthenticated_non_bff_endpoint_should_return_302_for_login()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/always_fail_authz_non_bff_endpoint"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Redirect);
            response.Headers.Location.ToString().ToLowerInvariant().Should().StartWith(_identityServerHost.Url("/connect/authorize"));
        }

        [Fact]
        public async Task unauthenticated_api_call_should_return_401()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/always_fail_authz"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
        
        [Fact]
        public async Task forbidden_api_call_should_return_403()
        {
            await _bffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/always_fail_authz"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task response_status_401_should_return_401()
        {
            await _bffHost.BffLoginAsync("alice");
            _bffHost.LocalApiStatusCodeToReturn = 401;

            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/local_authz"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task response_status_403_should_return_403()
        {
            await _bffHost.BffLoginAsync("alice");
            _bffHost.LocalApiStatusCodeToReturn = 403;

            var req = new HttpRequestMessage(HttpMethod.Get, _bffHost.Url("/local_authz"));
            req.Headers.Add("x-csrf", "1");
            var response = await _bffHost.BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
