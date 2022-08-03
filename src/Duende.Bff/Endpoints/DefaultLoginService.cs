// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Duende.Bff;

/// <summary>
/// Service for handling login requests
/// </summary>
public class DefaultLoginService : ILoginService
{
    /// <summary>
    /// The BFF options
    /// </summary>
    protected readonly BffOptions _options;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="options"></param>
    public DefaultLoginService(IOptions<BffOptions> options)
    {
        _options = options.Value;
    }
        
    /// <inheritdoc />
    public virtual async Task ProcessRequestAsync(HttpContext context)
    {
        context.CheckForBffMiddleware(_options);
            
        var returnUrl = context.Request.Query[Constants.RequestParameters.ReturnUrl].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            if (!Util.IsLocalUrl(returnUrl))
            {
                throw new Exception("returnUrl is not application local: " + returnUrl);
            }
        }

        if (String.IsNullOrWhiteSpace(returnUrl))
        {
            if (context.Request.PathBase.HasValue)
            {
                returnUrl = context.Request.PathBase;
            }
            else
            {
                returnUrl = "/";
            }
        }

        var props = new AuthenticationProperties
        {
            RedirectUri = returnUrl
        };

        await context.ChallengeAsync(props);
    }
}