using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Duende.Bff
{
    public class UserSessionsFilter
    {
        public string SubjectId { get; set; }
        public string SessionId { get; set; }

        internal void Validate()
        {
            if (String.IsNullOrWhiteSpace(SubjectId) && String.IsNullOrWhiteSpace(SessionId))
            {
                throw new ArgumentNullException("SubjectId or SessionId is required.");
            }
        }
    }

    public interface IUserSessionStore
    {
        Task<UserSession> GetUserSessionAsync(string key);
        Task CreateUserSessionAsync(UserSession ticket);
        Task UpdateUserSessionAsync(UserSession ticket);
        Task DeleteUserSessionAsync(string key);
        
        Task<IEnumerable<UserSession>> GetUserSessionsAsync(UserSessionsFilter filter);
        Task DeleteUserSessionsAsync(UserSessionsFilter filter);
    }

    public class InMemoryTicketStore : IUserSessionStore
    {
        ConcurrentDictionary<string, UserSession> _store = new ConcurrentDictionary<string, UserSession>();

        public Task CreateUserSessionAsync(UserSession ticket)
        {
            if (!_store.TryAdd(ticket.Key, ticket.Clone()))
            {
                throw new Exception("Key already exists");
            }
            return Task.CompletedTask;
        }

        public Task<UserSession> GetUserSessionAsync(string key)
        {
            _store.TryGetValue(key, out var item);
            return Task.FromResult(item?.Clone());
        }

        public Task UpdateUserSessionAsync(UserSession ticket)
        {
            _store[ticket.Key] = ticket.Clone();
            return Task.CompletedTask;
        }

        public Task DeleteUserSessionAsync(string key)
        {
            _store.TryRemove(key, out _);
            return Task.CompletedTask;
        }


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

    public class UserSession
    {
        public string Key { get; set; }

        public string SubjectId { get; set; }
        public string SessionId { get; set; }
        public string Scheme { get; set; }

        public DateTime Created { get; set; }
        public DateTime Renewed { get; set; }
        public DateTime? Expires { get; set; }

        public string Ticket { get; set; }

        internal UserSession Clone()
        {
            return new UserSession()
            {
                Key = this.Key,
                SubjectId = this.SubjectId,
                SessionId = this.SessionId,
                Scheme = this.Scheme,
                Created = this.Created,
                Renewed = this.Renewed,
                Expires = this.Expires,
                Ticket = this.Ticket,
            };
        }
    }
}
