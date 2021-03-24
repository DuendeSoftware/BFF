using FluentAssertions;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Duende.Bff.Tests.TestFramework
{
    public record ClaimRecord(string type, string value);

    public static class HttpClientExtensions
    {
        public static async Task<bool> GetIsUserLoggedInAsync(this TestHost host)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, host.Url("/bff/user"));
            req.Headers.Add("x-csrf", "1");
            var response = await host.BrowserClient.SendAsync(req);
            
            (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized).Should().BeTrue();
            
            return response.StatusCode == HttpStatusCode.OK;
        }
        
        public static async Task<ClaimRecord[]> GetUserClaimsAsync(this TestHost host)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, host.Url("/bff/user"));
            req.Headers.Add("x-csrf", "1");
            var response = await host.BrowserClient.SendAsync(req);
            return await response.ReadUserClaimsAsync();
        }

        public static async Task<ClaimRecord[]> ReadUserClaimsAsync(this HttpResponseMessage response)
        {
            response.StatusCode.Should().Be(200);

            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();

            var claims = JsonSerializer.Deserialize<ClaimRecord[]>(json);
            return claims;
        }
    }
}
