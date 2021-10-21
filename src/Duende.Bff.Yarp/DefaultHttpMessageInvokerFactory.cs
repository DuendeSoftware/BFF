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

                // propagates the current Activity to the downstream service
                // todo: has been removed in RC.1 - needs replacement
                // var handler = new ActivityPropagationHandler(
                //     ActivityContextHeaders.BaggageAndCorrelationContext, 
                //     socketsHandler);
                
                return new HttpMessageInvoker(socketsHandler);
            });
        }
    }
}