using System.Threading.Tasks;

namespace Duende.Bff
{
    public interface ISessionRevocationService
    {
        Task DeleteUserSessionsAsync(UserSessionsFilter filter);
    }
}