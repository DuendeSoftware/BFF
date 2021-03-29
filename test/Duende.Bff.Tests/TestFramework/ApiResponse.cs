// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Duende.Bff.Tests.TestFramework
{
    public record ApiResponse(string method, string path, string sub, string clientId, IEnumerable<ClaimRecord> claims, string body = null);
}
