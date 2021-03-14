using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Duende.Bff
{
    public class NopSessionRevocationService : ISessionRevocationService
    {
        private readonly ILogger<NopSessionRevocationService> _logger;

        public NopSessionRevocationService(ILogger<NopSessionRevocationService> logger)
        {
            _logger = logger;
        }

        public Task DeleteUserSessionsAsync(UserSessionsFilter filter)
        {
            _logger.LogDebug("Nop implementation of session revocation for sub: {sub}, and sid: {sid}. Implement ISessionRevocationService to provide your own implementation.", filter.SubjectId, filter.SessionId);
            return Task.CompletedTask;
        }
    }
}