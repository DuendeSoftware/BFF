// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System.Collections.Generic;
using Duende.Bff;
using Yarp.ReverseProxy.Abstractions;

namespace YARP.Sample
{
    public static class ProxyConfigExtensions
    {
        public static RouteConfig WithAccessToken(this RouteConfig config, TokenType tokenType)
        {
            Dictionary<string, string> metadata;

            if (config.Metadata != null)
            {
                metadata = new Dictionary<string, string>(config.Metadata);
            }
            else
            {
                metadata = new();
            }

            metadata.TryAdd("Duende.Bff.TokenType", tokenType.ToString());
            
            return config with { Metadata = metadata };
        }
    }
}