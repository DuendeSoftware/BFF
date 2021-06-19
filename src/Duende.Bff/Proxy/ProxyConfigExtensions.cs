// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System.Collections.Generic;
using Yarp.ReverseProxy.Configuration;

namespace Duende.Bff
{
    /// <summary>
    /// Extension methods for YARP configuration
    /// </summary>
    public static class ProxyConfigExtensions
    {
        /// <summary>
        /// Adds BFF access token metadata to a route configuration
        /// </summary>
        /// <param name="config"></param>
        /// <param name="tokenType"></param>
        /// <returns></returns>
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

            metadata.TryAdd(Constants.Yarp.TokenTypeMetadata, tokenType.ToString());
            
            return config with { Metadata = metadata };
        }
        
        /// <summary>
        /// Adds BFF access token metadata to a cluster configuration
        /// </summary>
        /// <param name="config"></param>
        /// <param name="tokenType"></param>
        /// <returns></returns>
        public static ClusterConfig WithAccessToken(this ClusterConfig config, TokenType tokenType)
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

            metadata.TryAdd(Constants.Yarp.TokenTypeMetadata, tokenType.ToString());
            
            return config with { Metadata = metadata };
        }
    }
}