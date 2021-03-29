// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;

namespace Duende.Bff
{
    /// <summary>
    /// IUserSession-backed ticket store
    /// </summary>
    public class ServerSideTicketStore : ITicketStore
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
                Scheme = ticket.AuthenticationScheme,
                Ticket = ticket.Serialize(),
            };

            await _store.CreateUserSessionAsync(session);

            return key;
        }

        /// <inheritdoc />
        public async Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            var session = await _store.GetUserSessionAsync(key);
            if (session != null)
            {
                var ticket = session.Deserialize();
                if (ticket == null)
                {
                    // if we failed to get a ticket, then remove DB record 
                    _logger.LogWarning("Failed to deserialize authentication ticket from store, deleting record for key {key}", key);
                    await RemoveAsync(key);
                }

                return ticket;
            }

            return null;
        }

        /// <inheritdoc />
        public async Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var session = await _store.GetUserSessionAsync(key);
            if (session != null)
            {
                session.Renewed = ticket.GetIssued();
                session.Expires = ticket.GetExpiration();
                session.Ticket = ticket.Serialize();

                // todo: discuss updating sub and sid?
                
                await _store.UpdateUserSessionAsync(session);
            }
            else
            {
                _logger.LogWarning("No record found in user session store when trying to renew authentication ticket for key {key} and subject {subjectId}", key, ticket.GetSubjectId());
            }
        }

        /// <inheritdoc />
        public Task RemoveAsync(string key)
        {
            return _store.DeleteUserSessionAsync(key);
        }
    }
}