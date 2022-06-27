// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

namespace Duende.Bff;

/// <summary>
/// Endpoint metadata for a remote BFF API endpoint
/// </summary>
public class BffRemoteApiEndpointMetadata : IBffApiEndpoint
{
    /// <summary>
    /// Required token type (if any)
    /// </summary>
    public TokenType? RequiredTokenType;
        
    /// <summary>
    /// Optionally send a user token if present
    /// </summary>
    public bool OptionalUserToken { get; set; }
        
    /// <summary>
    /// Maps to UserAccessTokenParameters and included if set
    /// </summary>
    public BffUserAccessTokenParameters? BffUserAccessTokenParameters { get; set; }
}