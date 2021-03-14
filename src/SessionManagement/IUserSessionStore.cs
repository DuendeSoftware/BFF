using System.Collections.Generic;
using System.Threading.Tasks;

namespace Duende.Bff
{
    public interface IUserSessionStore : ISessionRevocationService
    {
        Task<UserSession> GetUserSessionAsync(string key);
        Task CreateUserSessionAsync(UserSession ticket);
        Task UpdateUserSessionAsync(UserSession ticket);
        Task DeleteUserSessionAsync(string key);

        Task<IEnumerable<UserSession>> GetUserSessionsAsync(UserSessionsFilter filter);
    }
}