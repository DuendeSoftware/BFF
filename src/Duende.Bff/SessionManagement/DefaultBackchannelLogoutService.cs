// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Duende.Bff
{
    /// <summary>
    /// Default back-channel logout notification service implementation
    /// </summary>
    public class DefaultBackchannelLogoutService : IBackchannelLogoutService
    {
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private readonly IOptionsMonitor<OpenIdConnectOptions> _optionsMonitor;
        private readonly ISessionRevocationService _userSession;
        private readonly ILogger<DefaultBackchannelLogoutService> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="authenticationSchemeProvider"></param>
        /// <param name="optionsMonitor"></param>
        /// <param name="userSession"></param>
        /// <param name="logger"></param>
        public DefaultBackchannelLogoutService(
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            IOptionsMonitor<OpenIdConnectOptions> optionsMonitor,
            ISessionRevocationService userSession,
            ILogger<DefaultBackchannelLogoutService> logger)
        {
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _optionsMonitor = optionsMonitor;
            _userSession = userSession;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task ProcessRequequestAsync(HttpContext context)
        {
            context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
            context.Response.Headers.Add("Pragma", "no-cache");

            try
            {
                if (context.Request.HasFormContentType)
                {
                    var logoutToken = context.Request.Form[OidcConstants.BackChannelLogoutRequest.LogoutToken].FirstOrDefault();
                    
                    if (!String.IsNullOrWhiteSpace(logoutToken))
                    {
                        var user = await ValidateLogoutTokenAsync(logoutToken);
                        if (user != null)
                        {
                            // these are the sub & sid to signout
                            var sub = user.FindFirst("sub")?.Value;
                            var sid = user.FindFirst("sid")?.Value;

                            _logger.BackChannelLogout(sub ?? "missing", sid ?? "missing");
                            
                            await _userSession.DeleteUserSessionsAsync(new UserSessionsFilter 
                            { 
                                SubjectId = sub,
                                SessionId = sid
                            });
                            
                            return;
                        }
                    }
                    else
                    {
                        _logger.BackChannelLogoutError($"Failed to process backchannel logout request. 'Logout token is missing'");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.BackChannelLogoutError($"Failed to process backchannel logout request. '{ex.Message}'");
            }
            
            _logger.BackChannelLogoutError($"Failed to process backchannel logout request.");
            context.Response.StatusCode = 400;
        }

        private async Task<ClaimsIdentity> ValidateLogoutTokenAsync(string logoutToken)
        {
            var claims = await ValidateJwt(logoutToken);
            if (claims == null)
            {
                return null;
            }

            if (claims.FindFirst("sub") == null && claims.FindFirst("sid") == null)
            {
                _logger.BackChannelLogoutError("Logout token missing sub or sid claims.");
                return null;
            }

            var nonce = claims.FindFirst("nonce")?.Value;
            if (!String.IsNullOrWhiteSpace(nonce))
            {
                _logger.BackChannelLogoutError("Logout token should not contain nonce claim.");
                return null;
            }

            var eventsJson = claims.FindFirst("events")?.Value;
            if (String.IsNullOrWhiteSpace(eventsJson))
            {
                _logger.BackChannelLogoutError("Logout token missing events claim.");
                return null;
            }

            try
            {
                var events = JsonDocument.Parse(eventsJson);
                if (!events.RootElement.TryGetProperty("http://schemas.openid.net/event/backchannel-logout", out _))
                {
                    _logger.BackChannelLogoutError("Logout token contains missing http://schemas.openid.net/event/backchannel-logout value.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.BackChannelLogoutError($"Logout token contains invalid JSON in events claim value. {ex.Message}");
                return null;
            }

            return claims;
        }

        private async Task<ClaimsIdentity> ValidateJwt(string jwt)
        {
            var handler = new JsonWebTokenHandler();
            var parameters = await GetTokenValidationParameters();

            var result = handler.ValidateToken(jwt, parameters);
            if (result.IsValid)
            {
                return result.ClaimsIdentity;
            }

            _logger.BackChannelLogoutError(result.Exception.ToString());
            return null;
        }

        private async Task<TokenValidationParameters> GetTokenValidationParameters()
        {
            var scheme = await _authenticationSchemeProvider.GetDefaultChallengeSchemeAsync();
            if (scheme == null)
            {
                throw new Exception("Failed to obtain default challenge scheme");
            }

            var options = _optionsMonitor.Get(scheme.Name);
            if (options == null)
            {
                throw new Exception("Failed to obtain OIDC options for default challenge scheme");
            }

            var config = options.Configuration;
            if (config == null)
            {
                config = await options.ConfigurationManager.GetConfigurationAsync(CancellationToken.None);
            }

            if (config == null)
            {
                throw new Exception("Failed to obtain OIDC configuration");
            }

            var parameters = new TokenValidationParameters
            {
                ValidIssuer = config.Issuer,
                ValidAudience = options.ClientId,
                IssuerSigningKeys = config.SigningKeys,

                NameClaimType = JwtClaimTypes.Name,
                RoleClaimType = JwtClaimTypes.Role
            };

            return parameters;
        }
    }
}