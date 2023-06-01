// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Threading.Tasks;

namespace Duende.Bff;

/// <summary>
/// Retrieves access tokens
/// </summary>
public interface IAccessTokenRetriever
{
    /// <summary>
    /// Gets the access token
    /// </summary>
    /// <param name="context">Context used to retrieve the token</param>
    /// <returns></returns>
    Task<AccessTokenResult> GetAccessToken(AccessTokenRetrievalContext context);
}
