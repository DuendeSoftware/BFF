// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Duende.Bff
{
    /// <summary>
    /// Service for handling logout requests
    /// </summary>
    public class DefaultLogoutService : ILogoutService
    {
        private readonly IAuthenticationSchemeProvider _schemes;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="authenticationSchemeProvider"></param>
        public DefaultLogoutService(IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            _schemes = authenticationSchemeProvider;
        }

        /// <inheritdoc />
        public async Task ProcessRequequestAsync(HttpContext context)
        {
            // get rid of local cookie first
            var signInScheme = await _schemes.GetDefaultSignInSchemeAsync();
            await context.SignOutAsync(signInScheme.Name);

            var returnUrl = context.Request.Query["returnUrl"].FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                if (!Util.IsLocalUrl(returnUrl))
                {
                    throw new Exception("returnUrl is not application local");
                }
            }

            var props = new AuthenticationProperties
            {
                RedirectUri = returnUrl ?? "/"
            };

            // trigger idp logout
            await context.SignOutAsync(props);
        }
    }
}