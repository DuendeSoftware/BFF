// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Duende.Bff
{
    /// <summary>
    /// User session store
    /// </summary>
    public interface IUserSessionStore : ISessionRevocationService
    {
        /// <summary>
        /// Retrieves a user session
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<UserSession> GetUserSessionAsync(string key);
        
        /// <summary>
        /// Creates a user session
        /// </summary>
        /// <param name="session"></param>
        /// <returns></returns>
        Task CreateUserSessionAsync(UserSession session);

        /// <summary>
        /// Updates a user session
        /// </summary>
        /// <param name="key"></param>
        /// <param name="session"></param>
        /// <returns></returns>
        Task UpdateUserSessionAsync(string key, UserSessionUpdate session);
        
        /// <summary>
        /// Deletes a user session
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        Task DeleteUserSessionAsync(string key);

        /// <summary>
        /// Queries user sessions
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task<IEnumerable<UserSession>> GetUserSessionsAsync(UserSessionsFilter filter);
    }
}