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
    public interface IBackchannelLogoutService
    {
        Task ProcessRequequestAsync(HttpContext context);
    }

    public class BackchannelLogoutService : IBackchannelLogoutService
    {
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;
        private readonly IOptionsMonitor<OpenIdConnectOptions> _optionsMonitor;
        private readonly IUserSessionStore _userSessionStore;
        private readonly ILogger<BackchannelLogoutService> _logger;

        public BackchannelLogoutService(
            IAuthenticationSchemeProvider authenticationSchemeProvider,
            IOptionsMonitor<OpenIdConnectOptions> optionsMonitor,
            IUserSessionStore userSessionStore,
            ILogger<BackchannelLogoutService> logger)
        {
            _authenticationSchemeProvider = authenticationSchemeProvider;
            _optionsMonitor = optionsMonitor;
            _userSessionStore = userSessionStore;
            _logger = logger;
        }

        public async Task ProcessRequequestAsync(HttpContext context)
        {
            context.Response.Headers.Add("Cache-Control", "no-cache, no-store");
            context.Response.Headers.Add("Pragma", "no-cache");

            try
            {
                if (context.Request.HasFormContentType)
                {
                    var logout_token = context.Request.Form["logout_token"].FirstOrDefault();
                    if (!String.IsNullOrWhiteSpace(logout_token))
                    {
                        var user = await ValidateLogoutTokenAsync(logout_token);
                        if (user != null)
                        {
                            // these are the sub & sid to signout
                            var sub = user.FindFirst("sub")?.Value;
                            var sid = user.FindFirst("sid")?.Value;

                            // we would now delete the sub/sid from the user session DB
                            _logger.LogInformation("Backchannel logout for sub: {sub}, and sid: {sid}", sub, sid);
                            
                            await _userSessionStore.DeleteUserSessionsAsync(new UserSessionsFilter { 
                                SubjectId = sub,
                                SessionId = sid,
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process backchannel logout request.");
            }

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
                _logger.LogError("Logout token missing sub or sid claims.");
                return null;
            }

            var nonce = claims.FindFirst("nonce")?.Value;
            if (!String.IsNullOrWhiteSpace(nonce))
            {
                _logger.LogError("Logout token should not contain nonce claim.");
                return null;
            }

            var eventsJson = claims.FindFirst("events")?.Value;
            if (String.IsNullOrWhiteSpace(eventsJson))
            {
                _logger.LogError("Logout token missing events claim.");
                return null;
            }

            try
            {
                var events = JsonDocument.Parse(eventsJson);
                if (!events.RootElement.TryGetProperty("http://schemas.openid.net/event/backchannel-logout", out _))
                {
                    _logger.LogError("Logout token contains missing http://schemas.openid.net/event/backchannel-logout value.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout token contains invalid JSON in events claim value.");
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
            else
            {
                _logger.LogError(result.Exception, "Logout token validation failed.");
            }

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
