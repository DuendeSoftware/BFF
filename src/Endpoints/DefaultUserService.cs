// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Duende.Bff
{
    /// <summary>
    /// Service for handling user requests
    /// </summary>
    public class DefaultUserService : IUserService
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="loggerFactory"></param>
        public DefaultUserService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger("Duende.Bff.BffApiEndpoint");
        }

        /// <inheritdoc />
        public async Task ProcessRequequestAsync(HttpContext context)
        {
            var antiForgeryHeader = context.Request.Headers[BffDefaults.AntiForgeryHeaderName].FirstOrDefault();
            if (antiForgeryHeader == null || antiForgeryHeader != BffDefaults.AntiForgeryHeaderValue)
            {
                _logger.AntiForgeryValidationFailed("user");

                context.Response.StatusCode = 401;
                return;
            }

            var result = await context.AuthenticateAsync();

            if (!result.Succeeded)
            {
                context.Response.StatusCode = 401;
            }
            else
            {
                var claims = result.Principal.Claims.Select(x => new { type = x.Type, value = x.Value });
                var json = JsonSerializer.Serialize(claims);

                context.Response.StatusCode = 200;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(json, Encoding.UTF8);
            }
        }
    }
}