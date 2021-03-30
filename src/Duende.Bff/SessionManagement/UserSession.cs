// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;

namespace Duende.Bff
{
    /// <summary>
    /// The data that can be updated for a user session
    /// </summary>
    public class UserSessionUpdate
    {
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

        /// <summary>
        /// Copies this instance into another
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public void CopyTo(UserSessionUpdate other)
        {
            other.Renewed = Renewed;
            other.Expires = Expires;
            other.Ticket = Ticket;
        }
    }

    /// <summary>
    /// A user session
    /// </summary>
    public class UserSession : UserSessionUpdate
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
        /// Clones the instance
        /// </summary>
        /// <returns></returns>
        public UserSession Clone()
        {
            var other = new UserSession();
            CopyTo(other);
            return other;
        }
        
        /// <summary>
        /// Copies this instance into another
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public void CopyTo(UserSession other)
        {
            other.Key = Key;
            other.SubjectId = SubjectId;
            other.SessionId = SessionId;
            other.Scheme = Scheme;
            other.Created = Created;
            other.Renewed = Renewed;
            other.Expires = Expires;
            other.Ticket = Ticket;
        }
    }
}
