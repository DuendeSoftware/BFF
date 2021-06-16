// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Yarp.ReverseProxy.Transforms;

namespace Duende.Bff
{
    /// <summary>
    /// Adds an access token to outgoing requests
    /// </summary>
    public class AccessTokenRequestTransform : RequestTransform
    {
        private readonly string _accessToken;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="accessToken"></param>
        public AccessTokenRequestTransform(string accessToken)
        {
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        }
        
        /// <inheritdoc />
        public override ValueTask ApplyAsync(RequestTransformContext context)
        {
            context.ProxyRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

            return default;
        }
    }
}