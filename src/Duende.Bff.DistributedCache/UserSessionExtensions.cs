// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

namespace Duende.Bff.EntityFramework
{
    internal static class UserSessionExtensions
    {
        public static Identifier GetKeyIdentifier(
            this UserSession session,
            string applicationDiscriminator) =>
            new Identifier (applicationDiscriminator, session.Key, IdentifierKind.Key);

        public static Identifier GetSessionIdentifier(
            this UserSession session,
            string applicationDiscriminator) =>
            new Identifier (applicationDiscriminator, session.SessionId, IdentifierKind.Session);

        public static Identifier GetSubjectIdentifier(
            this UserSession session,
            string applicationDiscriminator) =>
            new Identifier (applicationDiscriminator, session.SubjectId, IdentifierKind.Subject);
    }
}