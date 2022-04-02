// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Duende.Bff.EntityFramework.Interfaces
{
    /// <summary>
    /// Abstraction for the session context.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public interface ISessionDbContext : IDisposable
    {
        /// <summary>
        /// Gets or sets the users sessions.
        /// </summary>
        /// <value>
        /// The users sessions.
        /// </value>
        
        public DbSet<UserSessionEntity> UserSessions { get; set; }
        
        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}