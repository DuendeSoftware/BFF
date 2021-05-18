// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Net;
using System.Net.Http;

namespace Duende.Bff
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
                return new HttpMessageInvoker(new SocketsHttpHandler
                {
                    UseProxy = false,
                    AllowAutoRedirect = false,
                    AutomaticDecompression = DecompressionMethods.None,
                    UseCookies = false
                });
            });
        }
    }
}