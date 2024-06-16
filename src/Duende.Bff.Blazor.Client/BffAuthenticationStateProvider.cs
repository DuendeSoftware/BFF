﻿using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components;

namespace Duende.Bff.Blazor.Wasm;

public class BffAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly TimeSpan UserCacheRefreshInterval = TimeSpan.FromSeconds(60);

    private readonly HttpClient _client;
    private readonly ILogger<BffAuthenticationStateProvider> _logger;

    private DateTimeOffset _userLastCheck = DateTimeOffset.Now;
    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());

    public BffAuthenticationStateProvider(
        PersistentComponentState state,
        IHttpClientFactory factory,
        ILogger<BffAuthenticationStateProvider> logger)
    {
        _client = factory.CreateClient("BffAuthenticationStateProvider");
        _logger = logger;
        _cachedUser = GetPersistedUser(state);
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = await GetUser();
        var state = new AuthenticationState(user);

        // checks periodically for a session state change and fires event
        // this causes a round trip to the server
        // adjust the period accordingly if that feature is needed
        // TODO - Add configuration for this
        if (user!.Identity!.IsAuthenticated)
        {
            _logger.LogInformation("starting background check..");
            Timer? timer = null;

            timer = new Timer(async _ =>
            {
                var currentUser = await GetUser(false);
                if (currentUser!.Identity!.IsAuthenticated == false)
                {
                    _logger.LogInformation("user logged out");
                    NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(currentUser)));
                    if (timer != null)
                    {
                        await timer.DisposeAsync();
                    }
                }
            }, null, 1000, 5000);
        }

        return state;
    }

    private async ValueTask<ClaimsPrincipal> GetUser(bool useCache = true)
    {
        var now = DateTimeOffset.Now;
        if (useCache && now < _userLastCheck + UserCacheRefreshInterval)
        {
            _logger.LogDebug("Taking user from cache");
            return _cachedUser;
        }

        _logger.LogDebug("Fetching user");
        _cachedUser = await FetchUser();
        _userLastCheck = now;

        return _cachedUser;
    }

    record ClaimRecord(string Type, object Value);

    private async Task<ClaimsPrincipal> FetchUser()
    {
        try
        {
            _logger.LogInformation("Fetching user information.");
            var response = await _client.GetAsync("bff/user?slide=false");
            response.EnsureSuccessStatusCode();
            var claims = await response.Content.ReadFromJsonAsync<List<ClaimRecord>>();

            var identity = new ClaimsIdentity(
                nameof(BffAuthenticationStateProvider),
                "name",
                "role");

            if (claims != null)
            {
                foreach (var claim in claims)
                {
                    identity.AddClaim(new Claim(claim.Type, claim.Value.ToString() ?? "no value"));
                }    
            }

            return new ClaimsPrincipal(identity);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fetching user failed.");
        }

        return new ClaimsPrincipal(new ClaimsIdentity());
    }

    private ClaimsPrincipal GetPersistedUser(PersistentComponentState state)
    {
        if (!state.TryTakeFromJson<ClaimsPrincipalLite>(nameof(ClaimsPrincipalLite), out var lite) || lite is null)
        {
            _logger.LogDebug("Failed to load persisted user.");
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        _logger.LogDebug("Persisted user loaded.");

        return lite.ToClaimsPrincipal();
    }
}
