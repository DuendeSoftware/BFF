using System;

namespace Duende.Bff
{
    public class UserSession
    {
        public string Key { get; set; }

        public string SubjectId { get; set; }
        public string SessionId { get; set; }
        public string Scheme { get; set; }

        public DateTime Created { get; set; }
        public DateTime Renewed { get; set; }
        public DateTime? Expires { get; set; }

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
