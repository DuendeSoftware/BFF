// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Duende.Bff
{
    /// <summary>
    /// OIDC configuration to add silent login support
    /// </summary>
    public class PostConfigureOidcOptionsForSilentLogin : IPostConfigureOptions<OpenIdConnectOptions>
    {
        private readonly BffOpenIdConnectEvents _events;

        /// <summary>
        /// ctor
        /// </summary>
        public PostConfigureOidcOptionsForSilentLogin(ILoggerFactory logger)
        {
            _events = new BffOpenIdConnectEvents(logger.CreateLogger<BffOpenIdConnectEvents>());
        }

        /// <inheritdoc />
        public void PostConfigure(string name, OpenIdConnectOptions options)
        {
            options.Events.OnRedirectToIdentityProvider = CreateRedirectCallback(options.Events.OnRedirectToIdentityProvider);
            options.Events.OnMessageReceived = CreateMessageReceivedCallback(options.Events.OnMessageReceived);
            options.Events.OnAuthenticationFailed = CreateAuthenticationFailedCallback(options.Events.OnAuthenticationFailed);
        }

        private Func<RedirectContext, Task> CreateRedirectCallback(Func<RedirectContext, Task> inner)
        {
            async Task Callback(RedirectContext ctx)
            {
                if (!await _events.ProcessRedirectToIdentityProviderAsync(ctx))
                {
                    if (inner != null)
                    {
                        await inner.Invoke(ctx);
                    }
                }
            };

            return Callback;
        }

        private Func<MessageReceivedContext, Task> CreateMessageReceivedCallback(Func<MessageReceivedContext, Task> inner)
        {
            async Task Callback(MessageReceivedContext ctx)
            {
                if (!await _events.ProcessMessageReceivedAsync(ctx))
                {
                    if (inner != null)
                    {
                        await inner.Invoke(ctx);
                    }
                }
            };

            return Callback;
        }

        private Func<AuthenticationFailedContext, Task> CreateAuthenticationFailedCallback(Func<AuthenticationFailedContext, Task> inner)
        {
            async Task Callback(AuthenticationFailedContext ctx)
            {
                if (!await _events.ProcessAuthenticationFailedAsync(ctx))
                {
                    if (inner != null)
                    {
                        await inner.Invoke(ctx);
                    }
                }
            };

            return Callback;
        }
    }
}
