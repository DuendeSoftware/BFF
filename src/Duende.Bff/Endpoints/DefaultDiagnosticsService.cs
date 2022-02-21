// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Duende.Bff
{
    /// <summary>
    /// Default debug diagnostics service
    /// </summary>
    public class DefaultDiagnosticsService : IDiagnosticsService
    {
        private readonly IWebHostEnvironment _environment;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="environment"></param>
        public DefaultDiagnosticsService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        
        /// <inheritdoc />
        public async Task ProcessRequestAsync(HttpContext context)
        {
            if (!_environment.IsDevelopment())
            {
                context.Response.StatusCode = 404;
                return;
            }

            var usertoken = await context.GetUserAccessTokenAsync();
            var clientToken = await context.GetClientAccessTokenAsync();

            var info = new DiagnosticsInfo
            {
                UserAccessToken = usertoken,
                ClientAccessToken = clientToken
            };
            
#if NET6_0_OR_GREATER
            var options = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
#else
            var options = new JsonSerializerOptions
            {
                IgnoreNullValues = true
            };
#endif

            var json = JsonSerializer.Serialize(info, options);

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(json);
        }

        private class DiagnosticsInfo
        {
            public string? UserAccessToken { get; set; }
            public string? ClientAccessToken { get; set; }
        }
    }
}