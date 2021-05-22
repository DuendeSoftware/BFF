// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;

namespace Duende.Bff
{
    /// <summary>
    /// Extends ITicketStore with additional query APIs.
    /// </summary>
    public interface IServerTicketStore : ITicketStore
    {
        /// <summary>
        /// Returns the AuthenticationTickets for the UserSessionsFilter.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
        /// <returns></returns>
        Task<IReadOnlyCollection<AuthenticationTicket>> GetUserTicketsAsync(UserSessionsFilter filter, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// IUserSession-backed ticket store
    /// </summary>
    public class ServerSideTicketStore : IServerTicketStore
    {
        private readonly IUserSessionStore _store;
        private readonly ILogger<ServerSideTicketStore> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="store"></param>
        /// <param name="logger"></param>
        public ServerSideTicketStore(
            IUserSessionStore store,
            ILogger<ServerSideTicketStore> logger)
        {
            _store = store;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = CryptoRandom.CreateUniqueId(format: CryptoRandom.OutputFormat.Hex);

            var session = new UserSession
            {
                Key = key,
                Created = ticket.GetIssued(),
                Renewed = ticket.GetIssued(),
                Expires = ticket.GetExpiration(),
                SubjectId = ticket.GetSubjectId(),
                SessionId = ticket.GetSessionId(),
                Ticket = ticket.Serialize()
            };

            await _store.CreateUserSessionAsync(session);

            return key;
        }

        /// <inheritdoc />
        public async Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            var session = await _store.GetUserSessionAsync(key);
            if (session == null) return null;
            
            var ticket = session.Deserialize();
            if (ticket != null) return ticket;
                
            // if we failed to get a ticket, then remove DB record 
            _logger.LogWarning("Failed to deserialize authentication ticket from store, deleting record for key {key}", key);
            await RemoveAsync(key);

            return ticket;
        }

        /// <inheritdoc />
        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            // todo: discuss updating sub and sid?
            return _store.UpdateUserSessionAsync(key, new UserSessionUpdate {
                Renewed = ticket.GetIssued(),
                Expires = ticket.GetExpiration(),
                Ticket = ticket.Serialize()
            });
        }

        /// <inheritdoc />
        public Task RemoveAsync(string key)
        {
            return _store.DeleteUserSessionAsync(key);
        }

        /// <summary>
        /// Returns the AuthenticationTickets for the UserSessionsFilter.
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="cancellationToken">A token that can be used to request cancellation of the asynchronous operation.</param>
        /// <returns></returns>
        public async Task<IReadOnlyCollection<AuthenticationTicket>> GetUserTicketsAsync(UserSessionsFilter filter, CancellationToken cancellationToken)
        {
            var list = new List<AuthenticationTicket>();
            
            var sessions = await _store.GetUserSessionsAsync(filter, cancellationToken);
            foreach(var session in sessions)
            {
                var ticket = session.Deserialize();
                list.Add(ticket);
            }

            return list;
        }
    }
}