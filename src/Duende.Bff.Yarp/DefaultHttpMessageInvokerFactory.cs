// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Forwarder;

namespace Duende.Bff.Yarp
{
    /// <summary>
    /// Default implementation of the message invoker factory.
    /// This implementation creates one message invoke per remote API endpoint
    /// </summary>
    public class DefaultHttpMessageInvokerFactory : IHttpMessageInvokerFactory
    {
        /// <summary>
        /// Dictionary to cachen invoker instances
        /// </summary>
        protected readonly ConcurrentDictionary<string, HttpMessageInvoker> Clients = new();

        /// <inheritdoc />
        public virtual HttpMessageInvoker CreateClient(string localPath)
        {
            return Clients.GetOrAdd(localPath, (key) =>
            {
                var socketsHandler = new SocketsHttpHandler
                {
                    UseProxy = false,
                    AllowAutoRedirect = false,
                    AutomaticDecompression = DecompressionMethods.None,
                    UseCookies = false
                };

                
#if NETCOREAPP3_1 || NET5_0
                // propagates the current Activity to the downstream service on .NET Core 3.1 and 5.0
                var handler = new ActivityPropagationHandler(socketsHandler);
                return new HttpMessageInvoker(handler);
#else
                // for .NET 6+ this feature is built-in to SocketsHandler directly
                return new HttpMessageInvoker(socketsHandler);
#endif
            });
        }
    }
}