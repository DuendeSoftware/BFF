// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Duende.Bff
{
    internal static class Extensions
    {
        public static void CheckForBffMiddleware(this HttpContext context, BffOptions options)
        {
            if (options.EnforceBffMiddleware)
            {
                var found = context.Items.TryGetValue(Constants.BffMiddlewareMarker, out _);
                if (!found)
                {
                    throw new InvalidOperationException(
                        "The BFF middleware is missing in the pipeline. Add 'app.UseBff' after 'app.UseRouting' but before 'app.UseAuthorization'");
                }
            }
        }

        public static bool CheckAntiForgeryHeader(this HttpContext context, BffOptions options)
        {
            var antiForgeryHeader = context.Request.Headers[options.AntiForgeryHeaderName].FirstOrDefault();
            return antiForgeryHeader != null && antiForgeryHeader == options.AntiForgeryHeaderValue;
        }

        public static async Task<string> GetManagedAccessToken(this HttpContext context, TokenType tokenType, UserAccessTokenParameters? userAccessTokenParameters = null)
        {
            string token;

            if (tokenType == TokenType.User)
            {
                token = await context.GetUserAccessTokenAsync(userAccessTokenParameters);
            }
            else if (tokenType == TokenType.Client)
            {
                token = await context.GetClientAccessTokenAsync();
            }
            else
            {
                token = await context.GetUserAccessTokenAsync(userAccessTokenParameters);

                if (string.IsNullOrEmpty(token))
                {
                    token = await context.GetClientAccessTokenAsync();
                }
            }

            return token;
        }
    }
}