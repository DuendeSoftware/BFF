// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;

namespace Duende.Bff
{
    /// <summary>
    /// A user session
    /// </summary>
    public class UserSession
    {
        /// <summary>
        /// The key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The subject ID
        /// </summary>
        public string SubjectId { get; set; }
        
        /// <summary>
        /// The session ID
        /// </summary>
        public string SessionId { get; set; }
        
        /// <summary>
        /// The scheme
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// The creation time
        /// </summary>
        public DateTime Created { get; set; }
        
        /// <summary>
        /// The renewal time
        /// </summary>
        public DateTime Renewed { get; set; }
        
        /// <summary>
        /// The expiration time
        /// </summary>
        public DateTime? Expires { get; set; }

        /// <summary>
        /// The ticket
        /// </summary>
        public string Ticket { get; set; }

        internal UserSession Clone()
        {
            return new UserSession()
            {
                Key = this.Key,
                SubjectId = this.SubjectId,
                SessionId = this.SessionId,
                Scheme = this.Scheme,
                Created = this.Created,
                Renewed = this.Renewed,
                Expires = this.Expires,
                Ticket = this.Ticket,
            };
        }
    }
}
