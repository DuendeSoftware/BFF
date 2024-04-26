// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Sockets;
using System.Security.Claims;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Duende.Bff.Blazor;

/// <summary>
/// Simplified implementation of a server-side token store.
/// Probably want something more robust IRL
/// </summary>

public class ServerSideTokenStore(
    IUserSessionStore sessionStore, 
    IDataProtectionProvider dataProtectionProvider,
    ILogger<ServerSideTokenStore> logger) : IUserTokenStore
{
    private readonly IDataProtector protector = dataProtectionProvider.CreateProtector("Duende.Bff.ServerSideTicketStore"); // TODO - Replace with constant
    // TODO - Respect UserTokenRequestParameters
    public async Task<UserToken> GetTokenAsync(ClaimsPrincipal user, UserTokenRequestParameters? parameters = null)
    {
        var session = await GetSession(user);
        var ticket = session.Deserialize(protector, logger) ?? throw new InvalidOperationException("TODO");

        var expiration = ticket.Properties.GetTokenValue("expires_at");
        DateTimeOffset dtExpires = DateTimeOffset.MaxValue;
        if (expiration != null)
        {
            dtExpires = DateTimeOffset.Parse(expiration, CultureInfo.InvariantCulture);
        }

        // TODO - use scheme and resource in name, optionally. Copy logic from AccessTokenManagement
        // TODO - consider making that logic reusable from ATM
        var tokens = new UserToken
        {
            AccessToken = ticket.Properties.GetTokenValue(OpenIdConnectParameterNames.AccessToken),
            RefreshToken = ticket.Properties.GetTokenValue(OpenIdConnectParameterNames.RefreshToken),
            AccessTokenType = ticket.Properties.GetTokenValue(OpenIdConnectParameterNames.TokenType),
            DPoPJsonWebKey = ticket.Properties.GetTokenValue("dpop_proof_key"),
            Expiration = dtExpires
        };

        logger.LogTrace("Retrieved server side tokens with expiration {expiration}", dtExpires);


        return tokens;
    }

    private async Task<UserSession> GetSession(ClaimsPrincipal user)
    {
        var sub = user.FindFirst("sub")?.Value ?? throw new InvalidOperationException("no sub claim");
        var sid = user.FindFirst("sid")?.Value ?? throw new InvalidOperationException("no sid claim");

        var sessions = await sessionStore.GetUserSessionsAsync(new UserSessionsFilter
        {
            SubjectId = sub,
            SessionId = sid
        });

        // TODO - should we throw or return a UserToken object with the error property set?
        if (sessions.Count == 0) throw new InvalidOperationException("No ticket found");
        if (sessions.Count > 1) throw new InvalidOperationException("Multiple tickets found");

        return sessions.First();
    }

    // TODO - use scheme and resource in name, optionally. Copy logic from AccessTokenManagement
    // TODO - consider making that logic reusable from ATM
    public async Task StoreTokenAsync(ClaimsPrincipal user, UserToken token, UserTokenRequestParameters? parameters = null)
    {
        await UpdateTicket(user, ticket =>
        {
            ticket.Properties.Items[".Token." +OpenIdConnectParameterNames.AccessToken] = token.AccessToken;
            ticket.Properties.Items[".Token." + OpenIdConnectParameterNames.TokenType] = token.AccessTokenType;
            ticket.Properties.Items[".Token." + OpenIdConnectParameterNames.RefreshToken] = token.RefreshToken;
            ticket.Properties.Items[".Token.dpop_proof_key"] = token.DPoPJsonWebKey;
            ticket.Properties.Items[".Token.expires_at"] = token.Expiration.ToString("o", CultureInfo.InvariantCulture);
        });
    }

    // TODO - use scheme and resource in name, optionally. Copy logic from AccessTokenManagement
    // TODO - consider making that logic reusable from ATM
    public async Task ClearTokenAsync(ClaimsPrincipal user, UserTokenRequestParameters? parameters = null)
    {
        await UpdateTicket(user, ticket =>
        {
            ticket.Properties.Items.Remove(OpenIdConnectParameterNames.AccessToken);
            ticket.Properties.Items.Remove(OpenIdConnectParameterNames.TokenType);
            ticket.Properties.Items.Remove(OpenIdConnectParameterNames.RefreshToken);
            ticket.Properties.Items.Remove("dpop_proof_key");
            ticket.Properties.Items.Remove("expires_at");
        });
    }

    public async Task UpdateTicket(ClaimsPrincipal user, Action<AuthenticationTicket> updateAction)
    {
        var session = await GetSession(user);
        var ticket = session.Deserialize(protector, logger) ?? throw new InvalidOperationException("TODO");

        updateAction(ticket);

        session.Ticket = ticket.Serialize(protector);

        await sessionStore.UpdateUserSessionAsync(session.Key, session);
    }
}
