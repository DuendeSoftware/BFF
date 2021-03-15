using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Duende.Bff
{
    /// <summary>
    /// In-memory user session store
    /// </summary>
    public class InMemoryUserSessionStore : IUserSessionStore
    {
        private readonly ConcurrentDictionary<string, UserSession> _store = new ConcurrentDictionary<string, UserSession>();

        /// <inheritdoc />
        public Task CreateUserSessionAsync(UserSession session)
        {
            if (!_store.TryAdd(session.Key, session.Clone()))
            {
                throw new Exception("Key already exists");
            }
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task<UserSession> GetUserSessionAsync(string key)
        {
            _store.TryGetValue(key, out var item);
            return Task.FromResult(item?.Clone());
        }

        /// <inheritdoc />
        public Task UpdateUserSessionAsync(UserSession session)
        {
            _store[session.Key] = session.Clone();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task DeleteUserSessionAsync(string key)
        {
            _store.TryRemove(key, out _);
            return Task.CompletedTask;
        }
        
        /// <inheritdoc />
        public Task<IEnumerable<UserSession>> GetUserSessionsAsync(UserSessionsFilter filter)
        {
            filter.Validate();

            var query = _store.Values.AsQueryable();
            if (!String.IsNullOrWhiteSpace(filter.SubjectId))
            {
                query = query.Where(x => x.SubjectId == filter.SubjectId);
            }
            if (!String.IsNullOrWhiteSpace(filter.SessionId))
            {
                query = query.Where(x => x.SessionId == filter.SessionId);
            }

            var results = query.Select(x => x.Clone()).ToArray().AsEnumerable();
            return Task.FromResult(results);
        }

        /// <inheritdoc />
        public Task DeleteUserSessionsAsync(UserSessionsFilter filter)
        {
            filter.Validate();

            var query = _store.Values.AsQueryable();
            if (!String.IsNullOrWhiteSpace(filter.SubjectId))
            {
                query = query.Where(x => x.SubjectId == filter.SubjectId);
            }
            if (!String.IsNullOrWhiteSpace(filter.SessionId))
            {
                query = query.Where(x => x.SessionId == filter.SessionId);
            }

            var keys = query.Select(x => x.Key).ToArray();

            foreach(var key in keys)
            {
                _store.TryRemove(key, out _);
            }

            return Task.CompletedTask;
        }
    }
}