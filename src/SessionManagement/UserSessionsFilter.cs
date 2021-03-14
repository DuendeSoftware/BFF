using System;

namespace Duende.Bff
{
    public class UserSessionsFilter
    {
        public string SubjectId { get; set; }
        public string SessionId { get; set; }

        internal void Validate()
        {
            if (String.IsNullOrWhiteSpace(SubjectId) && String.IsNullOrWhiteSpace(SessionId))
            {
                throw new ArgumentNullException("SubjectId or SessionId is required.");
            }
        }
    }
}