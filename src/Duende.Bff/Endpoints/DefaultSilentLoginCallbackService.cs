// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Text;
using System.Threading.Tasks;

namespace Duende.Bff;

/// <summary>
/// Service for handling silent login callback requests
/// </summary>
public class DefaultSilentLoginCallbackService : ISilentLoginCallbackService
{
    private readonly BffOptions _options;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="options"></param>
    public DefaultSilentLoginCallbackService(IOptions<BffOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public async Task ProcessRequestAsync(HttpContext context)
    {
        context.CheckForBffMiddleware(_options);

        var result = (await context.AuthenticateAsync()).Succeeded ? "true" : "false";
        var json = $"{{source:'bff-silent-login', isLoggedIn:{result}}}";
            
        var nonce = CryptoRandom.CreateUniqueId(format:CryptoRandom.OutputFormat.Hex);
        var origin = $"{context.Request.Scheme}://{context.Request.Host}";
            
        var html = $"<script nonce='{nonce}'>window.parent.postMessage({json}, '{origin}');</script>";

        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/html";
            
        context.Response.Headers["Content-Security-Policy"] = $"script-src 'nonce-{nonce}';";
        context.Response.Headers["Cache-Control"] = "no-store, no-cache, max-age=0";
        context.Response.Headers["Pragma"] = "no-cache";

        await context.Response.WriteAsync(html, Encoding.UTF8);
    }
}