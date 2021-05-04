// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Duende.Bff
{
    /// <summary>
    /// Default implementation of the ISessionRevocationService.
    /// </summary>
    public class SessionRevocationService : ISessionRevocationService
    {
        private readonly BffOptions _options;
        private readonly IServerTicketStore _ticketStore;
        private readonly IUserSessionStore _sessionStore;
        private readonly ITokenEndpointService _tokenEndpoint;
        private readonly ILogger<SessionRevocationService> _logger;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="ticketStore"></param>
        /// <param name="sessionStore"></param>
        /// <param name="tokenEndpoint"></param>
        /// <param name="logger"></param>
        public SessionRevocationService(BffOptions options, IServerTicketStore ticketStore, IUserSessionStore sessionStore, ITokenEndpointService tokenEndpoint, ILogger<SessionRevocationService> logger)
        {
            _options = options;
            _ticketStore = ticketStore;
            _sessionStore = sessionStore;
            _tokenEndpoint = tokenEndpoint;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task RevokeSessionsAsync(UserSessionsFilter filter)
        {
            if (_options.RevokeRefreshTokenOnLogout)
            {
                var tickets = await _ticketStore.GetUserTicketsAsync(filter);
                if (tickets?.Any() == true)
                {
                    foreach (var ticket in tickets)
                    {
                        var refreshToken = ticket.Properties.GetTokenValue("refresh_token");
                        if (!String.IsNullOrWhiteSpace(refreshToken))
                        {
                            var response = await _tokenEndpoint.RevokeRefreshTokenAsync(refreshToken);
                            if (response.IsError)
                            {
                                _logger.LogError("Error revoking refresh token: {error} for subject id: {sub} and session id: {sid}", response.Error, ticket.GetSubjectId(), ticket.GetSessionId());
                            }
                            else
                            {
                                _logger.LogDebug("Refresh token revoked successfully for subject id: {sub} and session id: {sid}", ticket.GetSubjectId(), ticket.GetSessionId());
                            }
                        }
                    }
                }
            }

            await _sessionStore.DeleteUserSessionsAsync(filter);
        }
    }
}