using System.Threading.Tasks;

namespace Duende.Bff
{
    /// <summary>
    /// Session revocation service
    /// </summary>
    public interface ISessionRevocationService
    {
        /// <summary>
        /// Deletes a user session
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Task DeleteUserSessionsAsync(UserSessionsFilter filter);
    }
}