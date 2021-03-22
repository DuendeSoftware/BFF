using Duende.Bff.Tests.TestFramework;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.Tests.Endpoints.Management
{
    public class UserEndpointTests
    {
        private readonly TestHost _host;

        public UserEndpointTests()
        {
            _host = new TestHost();
            _host.OnConfigureServices += ConfigureServices;
            _host.OnConfigure += Configure;
            _host.InitializeAsync().Wait();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddBff();
            services.AddAuthentication("cookie")
                .AddCookie("cookie");
        }
        
        private void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();
            app.UseRouting();
            
            app.UseAuthorization();

            app.UseEndpoints(endpoints => 
            {
                endpoints.MapBffManagementEndpoints();
            });
        }

        [Fact]
        public async Task test()
        {
            await _host.SignInAsync(new Claim("sub", "alice"), new Claim("foo", "foo1"), new Claim("foo", "foo2"));


            var req = new HttpRequestMessage(HttpMethod.Get, _host.Url("/bff/user"));
            req.Headers.Add("x-csrf", "1");
            var response = await _host.BrowserClient.SendAsync(req);
            
            response.StatusCode.Should().Be(200);
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);
            doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array);
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                item.ValueKind.Should().Be(JsonValueKind.Object);
                //var type = item.GetProperty("type").GetString();
                //var value = item.GetProperty("value").GetString();
            }
        }
    }
}
