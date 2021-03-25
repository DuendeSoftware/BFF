using System.Threading.Tasks;

namespace Duende.Bff.Tests.TestFramework
{
    public class MockSessionRevocationService : ISessionRevocationService
    {
        public bool DeleteUserSessionsWasCalled { get; set; }
        public UserSessionsFilter DeleteUserSessionsFilter { get; set; }
        public Task DeleteUserSessionsAsync(UserSessionsFilter filter)
        {
            DeleteUserSessionsWasCalled = true;
            DeleteUserSessionsFilter = filter;
            return Task.CompletedTask;
        }
    }
}
