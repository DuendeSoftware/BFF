// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Duende.Bff.EntityFramework
{
    internal static class UserSessionCacheExtensions
    {
        public static async Task StoreUserSessionAsync(
            this IDistributedCache distributedCache,
            UserSession session,
            string applicationDiscriminator,
            CancellationToken cancellationToken)
        {
            Identifier identifier = session.GetKeyIdentifier(applicationDiscriminator);
            Identifier subjectId = session.GetSubjectIdentifier(applicationDiscriminator);
            Identifier sessionId = session.GetSessionIdentifier(applicationDiscriminator);

            string? serializedSession = session.ToJson();

            DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = session.Expires ?? DateTimeOffset.Now.AddDays(1)
            };

            await Task.WhenAll(
                distributedCache
                    .SetStringAsync(identifier, serializedSession, cacheOptions, cancellationToken),
                distributedCache
                    .SetStringAsync(subjectId, identifier, cacheOptions, cancellationToken),
                distributedCache
                    .SetStringAsync(sessionId, identifier, cacheOptions, cancellationToken));
        }

        public static async Task<UserSession?> GetUserSessionAsync(
            this IDistributedCache cache,
            Identifier identifier,
            CancellationToken cancellationToken)
        {
            switch (identifier.Kind)
            {
                case IdentifierKind.Key:
                    string? result = await cache.GetStringAsync(identifier, cancellationToken);
                    if (result is null)
                    {
                        return null;
                    }

                    return UserSessionSerializer.FromJson(result);
                case IdentifierKind.Session:
                case IdentifierKind.Subject:
                    string? key = await cache.GetStringAsync(identifier, cancellationToken);
                    if (key is null || !Identifier.TryParse(key, out var parsed))
                    {
                        return null;
                    }

                    return await cache.GetUserSessionAsync(parsed.Value, cancellationToken);
                default:
                    return null;
            }
        }
    }
}