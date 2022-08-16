// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace Duende.Bff;

/// <summary>
/// Service for handling silent login requests
/// </summary>
public class DefaultSilentLoginService : ISilentLoginService
{
    private readonly BffOptions _options;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="options"></param>
    public DefaultSilentLoginService(IOptions<BffOptions> options)
    {
        _options = options.Value;
    }
        
    /// <inheritdoc />
    public async Task ProcessRequestAsync(HttpContext context)
    {
        context.CheckForBffMiddleware(_options);

        var pathBase = context.Request.PathBase;
        var redirectPath = pathBase + _options.SilentLoginCallbackPath;

        var props = new AuthenticationProperties
        {
            RedirectUri = redirectPath,
            Items =
            {
                { Constants.BffFlags.SilentLogin, "true" }
            },
        };

        await context.ChallengeAsync(props);
    }
}