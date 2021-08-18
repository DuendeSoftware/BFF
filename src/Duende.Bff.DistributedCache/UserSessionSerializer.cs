// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System.Text.Json;

namespace Duende.Bff.EntityFramework
{
    internal static class UserSessionSerializer
    {
        public static UserSession? FromJson(string? @string) => @string is null
            ? null
            : JsonSerializer.Deserialize<UserSession>(@string);

        public static string? ToJson(this UserSession? session) =>
            session is null ? null : JsonSerializer.Serialize(session);
    }
}