using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Builder
{
    internal static class EventIds
    {
        public static readonly EventId AccessTokenMissing = new EventId(1, "AccessTokenMissing");
        public static readonly EventId AntiforgeryValidationFailed = new EventId(2, "AntiforgeryValidationFailed");
        public static readonly EventId ProxyResponseError = new EventId(3, "ProxyResponseError");
    }
}