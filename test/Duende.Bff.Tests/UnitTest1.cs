using Duende.Bff.Tests.TestFramework;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Duende.Bff.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var host = new GenericHost();
            host.OnConfigure += app => app.Run(ctx => {
                ctx.Response.StatusCode = 204;
                return Task.CompletedTask;
            });
            await host.InitializeAsync();

            var response = await host.HttpClient.GetAsync("/test");

            response.StatusCode.Should().Be(204);
        }
    }
}
