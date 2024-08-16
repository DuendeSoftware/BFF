﻿// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Security.Claims;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;

namespace Duende.Bff.Blazor;

/// <summary>
///     A token store that retrieves tokens from server side sessions.
/// </summary>
public class ServerSideTokenStore(
    IStoreTokensInAuthenticationProperties tokensInAuthProperties,
    IUserSessionStore sessionStore,
    IDataProtectionProvider dataProtectionProvider,
    ILogger<ServerSideTokenStore> logger) : IUserTokenStore
{
    private readonly IDataProtector protector =
        dataProtectionProvider.CreateProtector(ServerSideTicketStore.DataProtectorPurpose);

    public async Task<UserToken> GetTokenAsync(ClaimsPrincipal user, UserTokenRequestParameters? parameters = null)
    {
        logger.LogDebug("Retrieving token for user {user}", user.Identity?.Name);
        var session = await GetSession(user);
        var ticket = session.Deserialize(protector, logger) ??
                     throw new InvalidOperationException("Failed to deserialize authentication ticket from session");

        return tokensInAuthProperties.GetUserToken(ticket.Properties, parameters);
    }

    private async Task<UserSession> GetSession(ClaimsPrincipal user)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");
        var sid = user.FindFirst("sid")?.Value ?? throw new InvalidOperationException("no sid claim");

        logger.LogDebug("Retrieving session {sid} for sub {sub}", sid, sub);

        var sessions = await sessionStore.GetUserSessionsAsync(new UserSessionsFilter
        {
            SubjectId = sub,
            SessionId = sid
        });

        if (sessions.Count == 0) throw new InvalidOperationException("No ticket found");
        if (sessions.Count > 1) throw new InvalidOperationException("Multiple tickets found");

        return sessions.First();
    }

    public async Task StoreTokenAsync(ClaimsPrincipal user, UserToken token,
        UserTokenRequestParameters? parameters = null)
    {
        logger.LogDebug("Storing token for user {user}", user.Identity?.Name);
        await UpdateTicket(user,
            ticket => { tokensInAuthProperties.SetUserToken(token, ticket.Properties, parameters); });
    }

    public async Task ClearTokenAsync(ClaimsPrincipal user, UserTokenRequestParameters? parameters = null)
    {
        logger.LogDebug("Removing token for user {user}", user.Identity?.Name);
        await UpdateTicket(user, ticket => { tokensInAuthProperties.RemoveUserToken(ticket.Properties, parameters); });
    }

    protected async Task UpdateTicket(ClaimsPrincipal user, Action<AuthenticationTicket> updateAction)
    {
        var session = await GetSession(user);
        var ticket = session.Deserialize(protector, logger) ??
                     throw new InvalidOperationException("Failed to deserialize authentication ticket from session");

        updateAction(ticket);

        session.Ticket = ticket.Serialize(protector);

        await sessionStore.UpdateUserSessionAsync(session.Key, session);
    }
}