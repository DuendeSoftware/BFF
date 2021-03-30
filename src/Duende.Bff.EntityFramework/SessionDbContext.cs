// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Duende.Bff.EntityFramework
{
    /// <summary>
    /// DbContext for session entities
    /// </summary>
    public class SessionDbContext : DbContext
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="options"></param>
        public SessionDbContext(DbContextOptions options) : base(options)
        {
        }

        /// <summary>
        /// DbSet for user sessions
        /// </summary>
        public DbSet<UserSessionEntity> UserSessions { get; set; }

        /// <inheritdoc/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureUserSessionSchema(modelBuilder.Entity<UserSessionEntity>());
        }

        /// <summary>
        /// Allows controlling the schema for UserSessionEntity
        /// </summary>
        protected virtual void ConfigureUserSessionSchema(EntityTypeBuilder<UserSessionEntity> entity)
        {
            entity.HasKey(x => x.Id);
            
            entity.Property(x => x.Key).IsRequired().HasMaxLength(200);
            entity.Property(x => x.SubjectId).IsRequired().HasMaxLength(200);
            entity.Property(x => x.Ticket).IsRequired();
            entity.Property(x => x.Scheme).HasMaxLength(200);

            entity.HasIndex(x => x.Key).IsUnique();
            entity.HasIndex(x => new { x.SubjectId, x.SessionId }).IsUnique();
            entity.HasIndex(x => x.SessionId).IsUnique();
        }
    }
}
