using System.Collections.Generic;

namespace Duende.Bff.Tests.TestFramework
{
    public record ApiResponse(string method, string path, string sub, string clientId, IEnumerable<ClaimRecord> claims, string body = null);
}
