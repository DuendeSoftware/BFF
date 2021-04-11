// using System.Linq;
// using System.Net.Http;
// using System.Text.Json;
// using System.Threading.Tasks;
// using Duende.Bff.Tests.TestFramework;
// using Duende.Bff.Tests.TestHosts;
// using FluentAssertions;
// using Xunit;
//
// namespace Duende.Bff.Tests.Endpoints
// {
//     public class UseForwardedHeaderBffTests : BffIntegrationTestBase
//     {
//         public UseForwardedHeaderBffTests()
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
//         public async Task bff_no_forwarding_calls_to_local_endpoint_should_return_standard_host_name()
//         {
//             CreateBffHost(false);
//             
//             var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/local_anon"));
//             req.Headers.Add("x-csrf", "1");
//             var response = await BffHost.BrowserClient.SendAsync(req);
//
//             response.IsSuccessStatusCode.Should().BeTrue();
//             response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
//             var json = await response.Content.ReadAsStringAsync();
//             var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
//
//             var host = apiResult.RequestHeaders["Host"].Single();
//             host.Should().Be("bff");
//         }
//         
//         [Fact]
//         public async Task bff_no_forwarding_calls_to_local_endpoint_with_xforwarded_header_should_return_locsl_host_name()
//         {
//             CreateBffHost(false);
//             
//             var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/local_anon"));
//             req.Headers.Add("x-csrf", "1");
//             req.Headers.Add("X-Forwarded-Host", "bff.public");
//             var response = await BffHost.BrowserClient.SendAsync(req);
//
//             response.IsSuccessStatusCode.Should().BeTrue();
//             response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
//             var json = await response.Content.ReadAsStringAsync();
//             var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
//
//             var host = apiResult.RequestHeaders["Host"].Single();
//             host.Should().Be("bff");
//         }
//         
//         [Fact]
//         public async Task bff_use_forwarding_calls_to_local_endpoint_should_return_standard_host_name()
//         {
//             CreateBffHost(true);
//             
//             var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/local_anon"));
//             req.Headers.Add("x-csrf", "1");
//             var response = await BffHost.BrowserClient.SendAsync(req);
//
//             response.IsSuccessStatusCode.Should().BeTrue();
//             response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
//             var json = await response.Content.ReadAsStringAsync();
//             var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
//
//             var host = apiResult.RequestHeaders["Host"].Single();
//             host.Should().Be("bff");
//         }
//         
//         [Fact]
//         public async Task bff_no_forwarding_remote_endpoint_no_forwarding_should_return_local_host_name()
//         {
//             CreateBffHost(false);
//             CreateApiHost(false);
//             
//             var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
//             req.Headers.Add("x-csrf", "1");
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
//         [Fact]
//         public async Task bff_no_forwarding_remote_endpoint_no_forwarding_with_xforwarded_header_should_return_local_host_name()
//         {
//             CreateBffHost(false);
//             CreateApiHost(false);
//             
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
//         // [Fact]
//         // public async Task bff_no_forwarding_remote_endpoint_no_forwarding_with_xforwarded_header_should_return_locsl_host_name()
//         // {
//         //     CreateBffHost(true);
//         //     
//         //     var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/local_anon"));
//         //     req.Headers.Add("x-csrf", "1");
//         //     req.Headers.Add("X-Forwarded-Host", "bff.forwarded");
//         //     var response = await BffHost.BrowserClient.SendAsync(req);
//         //
//         //     response.IsSuccessStatusCode.Should().BeTrue();
//         //     response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
//         //     var json = await response.Content.ReadAsStringAsync();
//         //     var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
//         //
//         //     var host = apiResult.RequestHeaders["Host"].Single();
//         //     host.Should().Be("bff.forwarded");
//         // }
//         
//         
//         
//         // [Fact]
//         // public async Task calls_to_remote_endpoint_should_return_standard_host()
//         // {
//         //     var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_anon_only/test"));
//         //     req.Headers.Add("x-csrf", "1");
//         //     var response = await BffHost.BrowserClient.SendAsync(req);
//         //
//         //     response.IsSuccessStatusCode.Should().BeTrue();
//         //     response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
//         //     var json = await response.Content.ReadAsStringAsync();
//         //     var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
//         //     
//         //     var host = apiResult.RequestHeaders["Host"].Single();
//         //     host.Should().Be("api");
//         // }
//         
//         // local API constructs a URL?
//         
//         // bff - useForwardedHeader false - no adding of forwarded headers - API useForwardedHeader false - value of host?
//         // bff - useForwardedHeader false - no adding of forwarded headers - API useForwardedHeader true - value of host?
//         
//         // bff - no forwarding - adding of forwarded headers - API useForwardedHeader true/false - value of host?
//         
//         // bff - no forwarding - receives forwarded headers - adding of forwarded headers, no append - API useForwardedHeader true/false - value of host?
//         // bff - no forwarding - receives forwarded headers - adding of forwarded headers, with append - API useForwardedHeader true/false - value of host?
//         
//         
//         
//         
//         
//         
//         
//     }
// }