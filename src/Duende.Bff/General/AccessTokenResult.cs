// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.AccessTokenManagement;

namespace Duende.Bff;

/// <summary>
/// Represents the result of attempting to obtain an access token.
/// </summary>
public class AccessTokenResult
{
    /// <summary>
    /// Flag that indicates if access token retrieval failed.
    /// </summary>
    public bool IsError { get; set; }

    /// <summary>
    /// The access token, or null if an error occurred or an optional token was
    /// not found.
    /// </summary>
    public ClientCredentialsToken? Token { get; set; } 
    // REVIEW: Should we add a more generically named type that
    // ClientCredentialsToken derives from? This type might confuse callers.
}
