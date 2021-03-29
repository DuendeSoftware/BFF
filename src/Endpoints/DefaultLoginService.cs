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
    /// Service for handling login requests
    /// </summary>
    public class DefaultLoginService : ILoginService
    {
        /// <inheritdoc />
        public async Task ProcessRequequestAsync(HttpContext context)
        {
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

            await context.ChallengeAsync(props);
        }
    }
}