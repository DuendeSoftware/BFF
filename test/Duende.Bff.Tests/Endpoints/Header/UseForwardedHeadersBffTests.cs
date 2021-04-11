// using System.Linq;
// using System.Net.Http;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Duende.Bff.Tests.TestFramework;
// using Duende.Bff.Tests.TestHosts;
// using FluentAssertions;
// using Xunit;
//
// namespace Duende.Bff.Tests.Headers
// {
//     public class UseForwardedHeadersBffTests : BffIntegrationTestBase
//     {
//         public UseForwardedHeadersBffTests()
//         {
//             BffHost = new BffHost(IdentityServerHost, ApiHost, "spa", "https://bff", useForwardedHeaders: true);
//             BffHost.InitializeAsync().Wait();
//         }
//         
//         private void CreateApiHost(bool useForwardedHeaders)
//         {
//             ApiHost = new ApiHost(IdentityServerHost, "scope1", "https://api", useForwardedHeaders);
//             ApiHost.InitializeAsync().Wait();
//         }
//         
//         [Fact]
//         public async Task local_endpoint_without_forwarded_headers_should_receive_standard_values()
//         {
//             var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/local_anon"));
//             req.Headers.Add("x-csrf", "1");
//             var response = await BffHost.BrowserClient.SendAsync(req);
//
//             response.IsSuccessStatusCode.Should().BeTrue();
//             var json = await response.Content.ReadAsStringAsync();
//             var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
//
//             var host = apiResult.RequestHeaders["Host"].Single();
//             host.Should().Be("bff");
//         }
//         
//         [Fact]
//         public async Task local_endpoint_with_forwarded_headers_should_receive_forwarded_values()
//         {
//             var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/local_anon"));
//             req.Headers.Add("x-csrf", "1");
//             req.Headers.Add("X-Forwarded-Host", "bff.forwarded");
//             var response = await BffHost.BrowserClient.SendAsync(req);
//
//             response.IsSuccessStatusCode.Should().BeTrue();
//             var json = await response.Content.ReadAsStringAsync();
//             var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
//
//             var host = apiResult.RequestHeaders["Host"].Single();
//             host.Should().Be("bff.forwarded");
//         }
//         
//         [Fact]
//         public async Task remote_endpoint_without_xforwarded_creation_should_receive_minimal_headers()
//         {
//             BffHost.BffOptions.AddXForwardedHeaders = false;
//             await BffHost.InitializeAsync();
//             
//             var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
//             req.Headers.Add("x-csrf", "1");
//             var response = await BffHost.BrowserClient.SendAsync(req);
//
//             response.IsSuccessStatusCode.Should().BeTrue();
//             var json = await response.Content.ReadAsStringAsync();
//             var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
//
//             apiResult.RequestHeaders.Count.Should().Be(1);
//
//             var host = apiResult.RequestHeaders.First().Value.Single();
//             host.Should().Be("api");
//         }
//         
//         
//         [Fact]
//         public async Task remote_endpoint_with_xforwarded_creation_should_receive_minimal_headers()
//         {
//             BffHost.BffOptions.AddXForwardedHeaders = true;
//             await BffHost.InitializeAsync();
//             
//             var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
//             req.Headers.Add("x-csrf", "1");
//             var response = await BffHost.BrowserClient.SendAsync(req);
//
//             response.IsSuccessStatusCode.Should().BeTrue();
//             var json = await response.Content.ReadAsStringAsync();
//             var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
//
//             apiResult.RequestHeaders.Count.Should().Be(3);
//             
//             apiResult.RequestHeaders["Host"].Single().Should().Be("api");
//             apiResult.RequestHeaders["X-Forwarded-Host"].Single().Should().Be("bff");
//             apiResult.RequestHeaders["X-Forwarded-Proto"].Single().Should().Be("https");
//         }
//         
//         [Fact]
//         public async Task remote_endpoint_with_forwarded_headers_should_return_real_host_name()
//         {
//             var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
//             req.Headers.Add("x-csrf", "1");
//             req.Headers.Add("X-Forwarded-Host", "bff.forwarded");
//             var response = await BffHost.BrowserClient.SendAsync(req);
//
//             response.IsSuccessStatusCode.Should().BeTrue();
//             var json = await response.Content.ReadAsStringAsync();
//             var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
//
//             var host = apiResult.RequestHeaders["Host"].Single();
//             host.Should().Be("api");
//         }
//         
//         [Fact]
//         public async Task remote_endpoint_with_forwarded_headers_and_xforwarded_creation_should_return_real_host_name()
//         {
//             var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
//             req.Headers.Add("x-csrf", "1");
//             req.Headers.Add("X-Forwarded-Host", "bff.forwarded");
//             var response = await BffHost.BrowserClient.SendAsync(req);
//
//             response.IsSuccessStatusCode.Should().BeTrue();
//             response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
//             var json = await response.Content.ReadAsStringAsync();
//             var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
//
//             var host = apiResult.RequestHeaders["Host"].Single();
//             host.Should().Be("api");
//         }
//         
//         
//     }
// }