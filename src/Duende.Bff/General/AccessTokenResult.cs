// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Duende.AccessTokenManagement;

namespace Duende.Bff;

/// <summary>
/// Represents the result of attempting to obtain an access token.
/// </summary>
// TODO "Result" isn't meaningful
public abstract class AccessTokenResult
{
}

public class AccessTokenError : AccessTokenResult
{
    public AccessTokenError(string error)
    {
        Error = error;
    }

    public string Error { get; set; }
}

public class OptionalAccessTokenNotFound : AccessTokenResult {}

// TODO - Split into files, add xmldoc
public class DPoPAccessToken : AccessTokenResult
{
    public DPoPAccessToken(string accessToken, string dpopJWK)
    {
        AccessToken = accessToken;
        DPoPJsonWebKey = dpopJWK;
    }

    /// <summary>
    /// The access token.
    /// </summary>
    public string AccessToken { get; set; } 
    

    /// <summary>
    /// The DPoP Json Web key
    /// </summary>
    public string DPoPJsonWebKey { get; set; }
}

public class BearerAccessToken : AccessTokenResult
{
    public BearerAccessToken(string accessToken)
    {
        AccessToken = accessToken;
    }
    /// <summary>
    /// The access token.
    /// </summary>
    public string AccessToken { get; private set; } 
}
