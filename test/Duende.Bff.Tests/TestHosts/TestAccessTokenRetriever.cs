// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Duende.Bff.Tests.TestHosts;

public class TestAccessTokenRetriever : IAccessTokenRetriever
{
    public TestAccessTokenRetriever(Func<Task<string>> accessTokenGetter)
    {
        _accessTokenGetter = accessTokenGetter;
    }

    private Func<Task<string>> _accessTokenGetter { get; set; }

    public async Task<AccessTokenResult> GetAccessToken(AccessTokenRetrievalContext context)
    {
        var token = await _accessTokenGetter();
        return new AccessTokenResult
        {
            IsError = false,
            Token = token
        };
    }
}
