// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using Duende.Bff.EntityFramework.Extensions;
using Duende.Bff.EntityFramework.Interfaces;
using Duende.Bff.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Duende.Bff.EntityFramework
{
    /// <summary>
    /// DbContext for session entities
    /// </summary>
    /// /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    /// <seealso cref="ISessionDbContext" />
    public class SessionDbContext : SessionDbContext<SessionDbContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionDbContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">storeOptions</exception>
        public SessionDbContext(DbContextOptions<SessionDbContext> options) : base(options)
        {
        }
    }
    
    /// <summary>
    /// DbContext for session entities
    /// </summary>
    /// /// <seealso cref="Microsoft.EntityFrameworkCore.DbContext" />
    /// <seealso cref="ISessionDbContext" />
    public class SessionDbContext<TContext> : DbContext, ISessionDbContext
        where TContext : DbContext, ISessionDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SessionDbContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">storeOptions</exception>
        public SessionDbContext(DbContextOptions<TContext> options) : base(options)
        {
        }
    
        /// <summary>
        /// DbSet for user sessions
        /// </summary>
        public DbSet<UserSessionEntity> UserSessions { get; set; }
    
        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var storeOptions = this.GetService<SessionStoreOptions>();
                
            if (storeOptions is null)
            {
                throw new ArgumentNullException(nameof(storeOptions));
            }
    
            modelBuilder.ConfigureSessionContext(storeOptions);
                
            base.OnModelCreating(modelBuilder);
        }
    }
}