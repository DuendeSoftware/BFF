﻿// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Duende.Bff.EntityFramework
{
    /// <summary>
    /// Entity framework core implementation of IUserSessionStore
    /// </summary>
    public class UserSessionStore : IUserSessionStore
    {
        private readonly string _applicationDiscriminator;
        private readonly SessionDbContext _sessionDbContext;
        private readonly ILogger<UserSessionStore> _logger;

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="options"></param>
        /// <param name="sessionDbContext"></param>
        /// <param name="logger"></param>
        public UserSessionStore(IOptions<DataProtectionOptions> options, SessionDbContext sessionDbContext, ILogger<UserSessionStore> logger)
        {
            _applicationDiscriminator = options.Value.ApplicationDiscriminator;
            _sessionDbContext = sessionDbContext;
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task CreateUserSessionAsync(UserSession session, CancellationToken cancellationToken)
        {
            var item = new UserSessionEntity()
            {
                ApplicationName = _applicationDiscriminator
            };
            session.CopyTo(item);
            _sessionDbContext.UserSessions.Add(item);
            return _sessionDbContext.SaveChangesAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task DeleteUserSessionAsync(string key, CancellationToken cancellationToken)
        {
            var items = await _sessionDbContext.UserSessions.Where(x => x.Key == key && x.ApplicationName == _applicationDiscriminator).ToArrayAsync(cancellationToken);
            var item = items.SingleOrDefault(x => x.Key == key && x.ApplicationName == _applicationDiscriminator);

            if (item != null)
            {
                _sessionDbContext.UserSessions.Remove(item);
                try
                {
                   await _sessionDbContext.SaveChangesAsync(cancellationToken);
                }
                catch(DbUpdateConcurrencyException ex)
                {
                    // suppressing exception for concurrent deletes
                    // https://github.com/DuendeSoftware/BFF/issues/63
                    _logger.LogDebug("DbUpdateConcurrencyException: {error}", ex.Message);
                    
                    foreach (var entry in ex.Entries)
                    {
                        // mark detatched so another call to SaveChangesAsync won't throw again
                        entry.State = EntityState.Detached;
                    }
                }
            }
            else
            {
                _logger.LogDebug("No record found in user session store when trying to delete user session for key {key}", key);
            }
        }

        /// <inheritdoc/>
        public async Task DeleteUserSessionsAsync(UserSessionsFilter filter, CancellationToken cancellationToken)
        {
            filter.Validate();

            var query = _sessionDbContext.UserSessions.Where(x => x.ApplicationName == _applicationDiscriminator).AsQueryable();
            if (!String.IsNullOrWhiteSpace(filter.SubjectId))
            {
                query = query.Where(x => x.SubjectId == filter.SubjectId);
            }
            if (!String.IsNullOrWhiteSpace(filter.SessionId))
            {
                query = query.Where(x => x.SessionId == filter.SessionId);
            }

            var items = await query.Where(x => x.ApplicationName == _applicationDiscriminator).ToArrayAsync(cancellationToken);
            if (!String.IsNullOrWhiteSpace(filter.SubjectId))
            {
                items = items.Where(x => x.SubjectId == filter.SubjectId).ToArray();
            }
            if (!String.IsNullOrWhiteSpace(filter.SessionId))
            {
                items = items.Where(x => x.SessionId == filter.SessionId).ToArray();
            }

            _sessionDbContext.RemoveRange(items);

            try
            {
                await _sessionDbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // suppressing exception for concurrent deletes
                // https://github.com/DuendeSoftware/BFF/issues/63
                _logger.LogDebug("DbUpdateConcurrencyException: {error}", ex.Message);

                foreach (var entry in ex.Entries)
                {
                    // mark detatched so another call to SaveChangesAsync won't throw again
                    entry.State = EntityState.Detached;
                }
            }
        }

        /// <inheritdoc/>
        public async Task<UserSession?> GetUserSessionAsync(string key, CancellationToken cancellationToken)
        {
            var items = await _sessionDbContext.UserSessions.Where(x => x.Key == key && x.ApplicationName == _applicationDiscriminator).ToArrayAsync(cancellationToken);
            var item = items.SingleOrDefault(x => x.Key == key && x.ApplicationName == _applicationDiscriminator);
            
            UserSession result = null;
            if (item != null)
            {
                result = new UserSession();
                item.CopyTo(result);
            }
            else
            {
                _logger.LogDebug("No record found in user session store when trying to get user session for key {key}", key);
            }

            return result;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyCollection<UserSession>> GetUserSessionsAsync(UserSessionsFilter filter, CancellationToken cancellationToken)
        {
            filter.Validate();

            var query = _sessionDbContext.UserSessions.Where(x => x.ApplicationName == _applicationDiscriminator).AsQueryable();
            if (!String.IsNullOrWhiteSpace(filter.SubjectId))
            {
                query = query.Where(x => x.SubjectId == filter.SubjectId);
            }
            if (!String.IsNullOrWhiteSpace(filter.SessionId))
            {
                query = query.Where(x => x.SessionId == filter.SessionId);
            }

            var items = await query.Where(x => x.ApplicationName == _applicationDiscriminator).ToArrayAsync(cancellationToken);
            if (!String.IsNullOrWhiteSpace(filter.SubjectId))
            {
                items = items.Where(x => x.SubjectId == filter.SubjectId).ToArray();
            }
            if (!String.IsNullOrWhiteSpace(filter.SessionId))
            {
                items = items.Where(x => x.SessionId == filter.SessionId).ToArray();
            }

            return items.Select(x => {
                var item = new UserSession();
                x.CopyTo(item);
                return item;
            }).ToArray();
        }

        /// <inheritdoc/>
        public async Task UpdateUserSessionAsync(string key, UserSessionUpdate session, CancellationToken cancellationToken)
        {
            var items = await _sessionDbContext.UserSessions.Where(x => x.Key == key && x.ApplicationName == _applicationDiscriminator).ToArrayAsync(cancellationToken);
            var item = items.SingleOrDefault(x => x.Key == key && x.ApplicationName == _applicationDiscriminator);
            if (item != null)
            {
                session.CopyTo(item);
                await _sessionDbContext.SaveChangesAsync(cancellationToken);
            }
            else
            {
                _logger.LogDebug("No record found in user session store when trying to update user session for key {key}", key);
            }
        }
    }
}
