// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Duende.Bff.EntityFramework
{
    /// <summary>
    /// Extensions for BffBuilder
    /// </summary>
    public static class BffBuilderExtensions
    {
        /// <summary>
        /// Adds entity framework core support for user session store.
        /// </summary>
        /// <param name="bffBuilder"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static BffBuilder AddEntityFrameworkServerSideSessions(this BffBuilder bffBuilder, Action<IServiceProvider, DbContextOptionsBuilder> action)
        {
            bffBuilder.Services.AddDbContext<SessionDbContext>(action);
            bffBuilder.Services.AddTransient<IUserSessionStore, UserSessionStore>();
            return bffBuilder.AddServerSideSessions();
        }

        /// <summary>
        /// Adds entity framework core support for user session store.
        /// </summary>
        /// <param name="bffBuilder"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static BffBuilder AddEntityFrameworkServerSideSessions(this BffBuilder bffBuilder, Action<DbContextOptionsBuilder> action)
        {
            bffBuilder.Services.AddDbContext<SessionDbContext>(action);
            bffBuilder.Services.AddTransient<IUserSessionStore, UserSessionStore>();
            return bffBuilder.AddServerSideSessions();
        }
    }
}
