// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
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
        private readonly string? _scheme;
        private readonly ILogger<PostConfigureOidcOptionsForSilentLogin> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="authenticationSchemeProvider"></param>
        /// <param name="logger"></param>
        public PostConfigureOidcOptionsForSilentLogin(IAuthenticationSchemeProvider authenticationSchemeProvider, ILogger<PostConfigureOidcOptionsForSilentLogin> logger)
        {
            _scheme = authenticationSchemeProvider.GetDefaultChallengeSchemeAsync().Result?.Name;
            _logger = logger;
        }

        /// <inheritdoc />
        public void PostConfigure(string name, OpenIdConnectOptions options)
        {
            if (name == _scheme)
            {
                options.Events.OnRedirectToIdentityProvider = CreateRedirectCallback(options.Events.OnRedirectToIdentityProvider);
                options.Events.OnMessageReceived = CreateMessageReceivedCallback(options.Events.OnMessageReceived);
                options.Events.OnAuthenticationFailed = CreateAuthenticationFailedCallback(options.Events.OnAuthenticationFailed);
            }
        }

        private Func<RedirectContext, Task> CreateRedirectCallback(Func<RedirectContext, Task> inner)
        {
            async Task Callback(RedirectContext ctx)
            {
                if (ctx.Properties!.Items.ContainsKey(Constants.BffFlags.SilentLogin))
                {
                    _logger.LogDebug("Setting OIDC ProtocolMessage.Prompt to 'none' for BFF silent login");
                    ctx.ProtocolMessage.Prompt = "none";
                }

                if (inner != null)
                {
                    await inner.Invoke(ctx);
                }
            };

            return Callback;
        }

        private Func<MessageReceivedContext, Task> CreateMessageReceivedCallback(Func<MessageReceivedContext, Task> inner)
        {
            async Task Callback(MessageReceivedContext ctx)
            {
                if (ctx.Properties!.Items.ContainsKey(Constants.BffFlags.SilentLogin) &&
                    ctx.Properties.RedirectUri != null)
                {
                    ctx.HttpContext.Items[Constants.BffFlags.SilentLogin] = ctx.Properties.RedirectUri;

                    if (ctx.ProtocolMessage.Error != null)
                    {
                        _logger.LogDebug("Handling error response from OIDC provider for BFF silent login.");
                        ctx.HandleResponse();
                        ctx.Response.Redirect(ctx.Properties.RedirectUri);
                        return;
                    }
                }

                if (inner != null)
                {
                    await inner.Invoke(ctx);
                }
            };

            return Callback;
        }

        private Func<AuthenticationFailedContext, Task> CreateAuthenticationFailedCallback(Func<AuthenticationFailedContext, Task> inner)
        {
            async Task Callback(AuthenticationFailedContext ctx)
            {
                if (ctx.HttpContext.Items.ContainsKey(Constants.BffFlags.SilentLogin))
                {
                    _logger.LogDebug("Handling failed response from OIDC provider for BFF silent login.");
                    ctx.HandleResponse();
                    ctx.Response.Redirect(ctx.HttpContext.Items[Constants.BffFlags.SilentLogin]!.ToString()!);
                    return;
                }

                if (inner != null)
                {
                    await inner.Invoke(ctx);
                }
            };

            return Callback;
        }
    }
}
