// // Copyright (c) Duende Software. All rights reserved.
// // See LICENSE in the project root for license information.

using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Blazor6.Client.BFF;

public class BffAuthenticationStateProvider : AuthenticationStateProvider
{
    private static readonly TimeSpan UserCacheRefreshInterval = TimeSpan.FromSeconds(60);

    private readonly NavigationManager _navigation;
    private readonly HttpClient _client;
    private readonly ILogger<BffAuthenticationStateProvider> _logger;

    private DateTimeOffset _userLastCheck = DateTimeOffset.FromUnixTimeSeconds(0);
    private ClaimsPrincipal _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());

    public BffAuthenticationStateProvider(NavigationManager navigation, HttpClient client,
        ILogger<BffAuthenticationStateProvider> logger)
    {
        _navigation = navigation;
        _client = client;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return new AuthenticationState(await GetUser(useCache: true));
    }

    private async ValueTask<ClaimsPrincipal> GetUser(bool useCache = false)
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
            var response = await _client.GetAsync("bff/user");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var claims = await response.Content.ReadFromJsonAsync<List<ClaimRecord>>();

                var identity = new ClaimsIdentity(
                    nameof(BffAuthenticationStateProvider),
                    "name",
                    "role");

                foreach (var claim in claims)
                {
                    identity.AddClaim(new Claim(claim.Type, claim.Value.ToString()));
                }

                return new ClaimsPrincipal(identity);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Fetching user failed.");
        }

        return new ClaimsPrincipal(new ClaimsIdentity());
    }
}