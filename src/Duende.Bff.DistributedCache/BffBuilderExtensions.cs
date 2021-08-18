// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Duende.Bff.EntityFramework;

namespace Duende.Bff.DistributedCache
{
    /// <summary>
    /// Extensions for BffBuilder
    /// </summary>
    public static class BffBuilderExtensions
    {
        /// <summary>
        /// Adds distributed cache support for user session store.
        /// </summary>
        /// <param name="bffBuilder"></param>
        /// <returns></returns>
        public static BffBuilder AddDistributedCacheServerSideSessions(this BffBuilder bffBuilder)
        {
            bffBuilder.Services.AddTransient<IUserSessionStore, DistributedCacheUserSessionStore>();
            return bffBuilder.AddServerSideSessions();
        }
    }
}