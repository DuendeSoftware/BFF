// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static Duende.Bff.EntityFramework.IdentifierKind;

namespace Duende.Bff.EntityFramework
{
    /// <summary>
    /// Entity framework core implementation of IUserSessionStore
    /// </summary>
    public class DistributedCacheUserSessionStore : IUserSessionStore
    {
        private readonly string _applicationDiscriminator;
        private readonly IDistributedCache _distributedCache;
        private readonly ILogger<DistributedCacheUserSessionStore> _logger;

        public DistributedCacheUserSessionStore(
            IOptions<DataProtectionOptions> options,
            IDistributedCache distributedCache,
            ILogger<DistributedCacheUserSessionStore> logger)
        {
            _applicationDiscriminator = options.Value.ApplicationDiscriminator;
            _distributedCache = distributedCache;
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task CreateUserSessionAsync(
            UserSession session,
            CancellationToken cancellationToken = default) =>
            _distributedCache
                .StoreUserSessionAsync(session, _applicationDiscriminator, cancellationToken);

        /// <inheritdoc/>
        public async Task DeleteUserSessionAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            UserSession? session = await GetUserSessionAsync(key, cancellationToken);

            if (session != null)
            {
                await DeleteUserSessionAsync(session, cancellationToken);
            }

            _logger.LogDebug(
                "No record found in user session store when trying to " +
                "delete user session for key {key}",
                key);
        }

        private async Task DeleteUserSessionAsync(
            UserSession session,
            CancellationToken cancellationToken = default)
        {
            Identifier key = session.GetKeyIdentifier(_applicationDiscriminator);
            Identifier subjectId = session.GetSubjectIdentifier(_applicationDiscriminator);
            Identifier sessionId = session.GetSessionIdentifier(_applicationDiscriminator);

            await Task.WhenAll(
                _distributedCache
                    .RemoveAsync(key, cancellationToken),
                _distributedCache
                    .RemoveAsync(subjectId, cancellationToken),
                _distributedCache
                    .RemoveAsync(sessionId, cancellationToken));
        }

        /// <inheritdoc/>
        public async Task DeleteUserSessionsAsync(
            UserSessionsFilter filter,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<UserSession> sessions =
                await GetUserSessionsAsync(filter, cancellationToken);

            await Task.WhenAll(
                sessions.Select(session => DeleteUserSessionAsync(session, cancellationToken)));
        }

        /// <inheritdoc/>
        public async Task<UserSession?> GetUserSessionAsync(
            string key,
            CancellationToken cancellationToken = default)
        {
            Identifier identifier =
                new Identifier(_applicationDiscriminator, key, Key);

            UserSession? session =
                await _distributedCache
                    .GetUserSessionAsync(identifier, cancellationToken);

            if (session is null)
            {
                _logger.LogDebug(
                    "No record found in user session" +
                    " store when trying to get user session for key {key}",
                    key);
            }

            return session;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<UserSession>> GetUserSessionsAsync(
            UserSessionsFilter filter,
            CancellationToken cancellationToken = default)
        {
            filter.Validate();

            UserSession? sessionBySubjectId = null;
            UserSession? sessionBySessionId = null;
            if (!string.IsNullOrWhiteSpace(filter.SubjectId))
            {
                Identifier identifier =
                    new Identifier(_applicationDiscriminator, filter.SubjectId, Subject);

                sessionBySubjectId =
                    await _distributedCache.GetUserSessionAsync(identifier, cancellationToken);
            }

            if (!string.IsNullOrWhiteSpace(filter.SessionId))
            {
                Identifier identifier =
                    new Identifier(_applicationDiscriminator, filter.SessionId, Session);

                sessionBySessionId =
                    await _distributedCache.GetUserSessionAsync(identifier, cancellationToken);
            }

            if (sessionBySessionId is null && sessionBySubjectId is null)
            {
                return Array.Empty<UserSession>();
            }

            if (sessionBySessionId is null || sessionBySubjectId is null ||
                sessionBySessionId.Key == sessionBySubjectId.Key)
            {
                return new[]
                {
                    sessionBySessionId ?? sessionBySubjectId!
                };
            }

            return new[]
            {
                sessionBySessionId,
                sessionBySubjectId!
            };
        }

        /// <inheritdoc/>
        public async Task UpdateUserSessionAsync(
            string key,
            UserSessionUpdate session,
            CancellationToken cancellationToken = default)
        {
            Identifier identifier = new Identifier(_applicationDiscriminator, key, Key);

            UserSession? userSession =
                await _distributedCache.GetUserSessionAsync(identifier, cancellationToken);

            if (userSession != null)
            {
                session.CopyTo(userSession);
                await _distributedCache.StoreUserSessionAsync(
                    userSession,
                    _applicationDiscriminator,
                    cancellationToken);
            }
            else
            {
                _logger.LogDebug(
                    "No record found in user session store when " +
                    "trying to update user session for key {key}",
                    key);
            }
        }
    }
}