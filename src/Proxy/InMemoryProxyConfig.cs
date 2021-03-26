// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Primitives;
using Microsoft.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Abstractions;
using Yarp.ReverseProxy.Service;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions methods for IReverseProxyBuilder
    /// </summary>
    public static class InMemoryConfigProviderExtensions
    {
        /// <summary>
        /// Loads the reverse proxy without any static configuration
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IReverseProxyBuilder LoadFromMemory(this IReverseProxyBuilder builder)
        {
            return builder.LoadFromMemory(
                new List<ProxyRoute>(),
                new List<Cluster>());
        }
        
        /// <summary>
        /// Loads the reverse proxy with a list of routes and clusters
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="routes"></param>
        /// <param name="clusters"></param>
        /// <returns></returns>
        public static IReverseProxyBuilder LoadFromMemory(this IReverseProxyBuilder builder, IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
        {
            builder.Services.AddSingleton<IProxyConfigProvider>(new InMemoryConfigProvider(routes, clusters));
            return builder;
        }
    }
}

namespace Microsoft.ReverseProxy.Configuration
{
    /// <inheritdoc />
    public class InMemoryConfigProvider : IProxyConfigProvider
    {
        private volatile InMemoryConfig _config;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="clusters"></param>
        public InMemoryConfigProvider(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
        {
            _config = new InMemoryConfig(routes, clusters);
        }

        /// <summary>
        /// Returns the in-memory configuration
        /// </summary>
        /// <returns></returns>
        public IProxyConfig GetConfig() => _config;

        /// <summary>
        /// Updates the in-memory configuration
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="clusters"></param>
        public void Update(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
        {
            var oldConfig = _config;
            _config = new InMemoryConfig(routes, clusters);
            oldConfig.SignalChange();
        }

        /// <summary>
        /// In-memory configuration
        /// </summary>
        private class InMemoryConfig : IProxyConfig
        {
            private readonly CancellationTokenSource _cts = new CancellationTokenSource();

            /// <summary>
            /// ctor
            /// </summary>
            /// <param name="routes"></param>
            /// <param name="clusters"></param>
            public InMemoryConfig(IReadOnlyList<ProxyRoute> routes, IReadOnlyList<Cluster> clusters)
            {
                Routes = routes;
                Clusters = clusters;
                ChangeToken = new CancellationChangeToken(_cts.Token);
            }

            public IReadOnlyList<ProxyRoute> Routes { get; }

            public IReadOnlyList<Cluster> Clusters { get; }

            public IChangeToken ChangeToken { get; }

            internal void SignalChange()
            {
                _cts.Cancel();
            }
        }
    }
}